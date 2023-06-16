CREATE OR REPLACE VIEW vw_agentprefs
(
    agentid,
    preference,
    score
)
as
    SELECT DISTINCT a.id,
        t.name AS preference,
        at.score
    FROM agents a,
        agent_tags at,
        tags t
    WHERE a.id = at.agentid
        AND at.tagid = t.id;

---

CREATE OR REPLACE VIEW vw_browsehistory
(
    agentid,
    user_id,
    item_id,
    category,
    timestamp
)
as
    SELECT a.id                                       AS agentid,
        a.cloudid                                  AS user_id,
        b.siteid                                   AS item_id,
        c.cats                                     AS category,
        round(date_part('epoch'::text, b.created)) AS "timestamp"
    FROM ml_agent_browse_history b,
        agents a,
        ml_categories c,
        ml_sites s
    WHERE a.id = b.agentid
        AND b.siteid = s.id
        AND s.domain = c.url
    ORDER BY (round(date_part('epoch'::text, b.created)));

---

CREATE OR REPLACE VIEW vw_browsehistory_random
(
    agentid,
    user_id,
    item_id,
    category,
    timestamp
)
as
    SELECT a.id                                       AS agentid,
        a.cloudid                                  AS user_id,
        b.siteid                                   AS item_id,
        c.cats                                     AS category,
        round(date_part('epoch'::text, b.created)) AS "timestamp"
    FROM ml_agent_browse_history_random b,
        agents a,
        ml_categories c,
        ml_sites s
    WHERE a.id = b.agentid
        AND b.siteid = s.id
        AND s.domain = c.url
    ORDER BY (round(date_part('epoch'::text, b.created)));

---

CREATE OR REPLACE procedure create_preferenced()
    language plpgsql
as
$$
DECLARE
    aid   uuid;
    pref  text;
    score integer;
    sid   integer;
    ct    integer;
BEGIN

    CREATE TABLE
    IF NOT EXISTS _t
    (
        id serial PRIMARY KEY,
        aa uuid,
        pp text
    );
COMMIT;

FOR aid, score, pref IN
select distinct a.id, at.score, t.name
from agents as a,
    agent_tags as at,
    tags as t
where a.id = at.agentid
    and at.tagid = t.id
LOOP

            FOR i IN 1..1000
                LOOP
                    if trunc(random() * 99 + 1) < score then
                    INSERT INTO _t
                        (aa, pp)
                    VALUES
                        (aid, pref);
                    else
                    INSERT INTO _t
                        (aa, pp)
                    VALUES
                        (aid, NULL);
                    end
                    if;
                END LOOP;
COMMIT;
END LOOP;

    FOR aid, pref, ct IN
SELECT aa, pp, count(*)
FROM _t
group by aa, pp
LOOP

    IF pref is NOT NULL then
    -- preference match
    INSERT INTO ml_agent_browse_history
        (agentid, siteid, created)
    select aid, s.id, NOW() - (random() * (interval '360 days'))
                    from ml_sites as s,
                         ml_categories as c
                    where s.domain = c.url
                      and s.id < 500000
                      and c.cats = pref
                    order by random()
                    limit ct;
                ELSE
    -- no pref match, random
    INSERT INTO ml_agent_browse_history
        (agentid, siteid, created)
    select aid, s.id, NOW() - (random() * (interval '360 days'))
                    from ml_sites as s,
                         ml_categories as c
                    where s.id < 500000
                      and s.domain = c.url
                      and c.cats not like '%|%'
                    order by random()
                    limit ct;
    END
    IF;
                raise notice '% % %', aid, pref, ct;
COMMIT;
END LOOP;

DROP TABLE _t;
COMMIT;

END;
$$;

---

alter procedure create_preferenced() owner to ghosts;

CREATE OR REPLACE procedure create_random()
    language plpgsql
as
$$
DECLARE
    aid uuid;
    sid integer;
BEGIN
    FOR aid IN
    select distinct agents.id as agentid
    from agents 
        LOOP
    for sid in
    select s.id
    from ml_sites as s,
        ml_categories as c
    where s.domain = c.url
        and s.id < 500000
        and c.cats not like '%|%'
    order by random()
                       limit 1000
                loop

    INSERT INTO ml_agent_browse_history_random
        (agentid, siteid, created)
    VALUES
        (aid, sid, NOW() - (random() * (interval '360 days')));

end loop;
END LOOP;
END;
$$;

---
