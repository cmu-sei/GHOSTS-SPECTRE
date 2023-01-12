# SPECTRE (GHOSTS Agent Preference Engine)

As GHOSTS agents make more informed, and hopefully, more complex decisions, there is a need for each agent to have a system of preferences existing at the time the agent is created, and for an ability to update those preferences over time as the agent continues to make decisions and measure the outcome of those decisions afterwards.

SPECTRE provides GHOSTS enables agents to make preferenced decisions and to use the outcome of those decisions to learn and evaluate future choices more intelligently.

[Documentation for SPECTRE is here](https://cmu-sei.github.io/GHOSTS/spectre/) with easy access to the rest of the GHOSTS framework as well.

## How SPECTRE works

SPECTRE currently has two components:

### PREFERENCE ENGINE

- An administrator creates the types of personas they wish to have GHOSTS agents represent
- GHOSTS agents report in their browser history to the GHOSTS C2 API
- If the agent has no persona, SPECTRE assigns one at random
- The persona assignment randomly creates different preferences for the agent based on persona settings (not unlike character creation in D&D)
- Preferences are a preference and its numeric value (-100 to 100) which roughly represents how much an agent likes or dislikes something
  - Preferences can be any key:value pair, such as:
    - "sports":50
    - "kali":35
    - "127.0.0.1":77
  - Preferences of 0 mean that the agent is indifferent (or has no preference)
  - All preference history is stored, so that in the (near) future we can track an agent's preferences over time
  - It is assumed that high negative preferences would have agents avoiding those values (e.g. for "pickles":-100 one would assume an agent is avoiding eating pickles at all costs.)

### MACHINE LEARNING

The incoming GHOSTS agent browsing activity can be attenuated to individual agent preferences after they are assigned some number of preferences based on default persona profiles. SPECTRE will aggregate this information periodically and perform model training and testing against that browsing activity, and recommend new browsing patterns for that agent to execute. This basically creates a new activity timeline for the agent. This cycle is referred to as a "Test". At the conclusion of any given test, that information would be used to inform the next round of ML testing done.

## License

[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.

Copyright 2020 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.
