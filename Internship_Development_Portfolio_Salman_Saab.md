# Development Portfolio
## CMGT Internship Year 3

---

**Student Name:** Salman Saab
**Student Number:** 461950
**Programme:** Creative Media and Game Technologies (CMGT)
**Internship Period:** September 2, 2025 - January 24, 2026
**Company:** DIT-Lab
**Company Supervisors:** Remco & Reimer
**Internship Coach:** [To be filled]

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Company Description](#2-company-description)
3. [Assignment Description](#3-assignment-description)
4. [Learning Goals](#4-learning-goals)
5. [Activity Logs & Reflections](#5-activity-logs--reflections)
6. [Competency Demonstrations](#6-competency-demonstrations)
7. [Learning Outcomes & Reflection](#7-learning-outcomes--reflection)
8. [Future Aspirations](#8-future-aspirations)
9. [Appendices](#9-appendices)

---

## 1. Introduction

This portfolio covers my 20-week internship at DIT-Lab where I worked as a backend developer on **Projekt Kiosk**, a narrative game about being a kiosk clerk making moral choices. My main job was refactoring the entire game architecture and building technical systems that made the game scalable and easier to work with.

When I started in September, the game was a basic prototype with 3 characters and hardcoded dialogue. I transformed it into a data-driven system with proper architecture, allowing our narrative designer Peter to create content independently without needing me to code everything.

**What I Actually Did:**
- Refactored entire game architecture from hardcoded to JSON-based
- Built backend systems (dialogue system, inbox, authorization, scoring)
- Created tools and documentation so non-coders could add content
- Organized project structure and assets
- Made the game scalable and maintainable
- Handled version control and repository management

I didn't write the story or design characters - that was Peter's work. My role was building the engine that powered his content.

---

## 2. Company Description

**Company:** DIT-Lab
**Website:** https://ditlab.nl/
**Type:** Public Innovation Lab

**What is DIT-Lab?**

DIT-Lab is a collaboration between DUO (Dutch Student Finance) and Hanze University. They're basically an innovation lab that explores how government services can stay relevant in the future. Their tagline is "Discover Innovations of Tomorrow" - they test new tech and ideas through research and experimentation.

**What They Do:**
- Research and test emerging technologies
- Build serious games and prototypes
- Work with students on innovative projects
- Help DUO stay ahead of changes in public services

**My Experience:**

I worked with Remco and Reimer as my stakeholders. The environment was pretty chill - they gave me freedom to make technical decisions but were available when I needed guidance. The focus was on learning and experimenting, which is why they brought in students like me.

The team included developers, narrative designers (like Peter), and people working on different game prototypes. It felt more like a startup vibe than a traditional company - lots of autonomy, room for mistakes, and focus on learning through doing.

---

## 3. Assignment Description

### Project: Projekt Kiosk

**Game Concept:**

Set in 2030, you're a kiosk clerk who checks IDs, sells items, and deals with customers offering bribes or acting suspicious. Your choices affect a corruption score - get too corrupt and the police arrest you. It's about moral choices in a mundane setting.

**When I Started (September 2025):**

The game existed as a basic prototype:
- 3 customers with hardcoded dialogue in Unity Inspector
- Simple linear conversations
- Basic checkout system
- Simple score screens
- Everything was manual and rigid

**The Problem:**

Adding new characters required tons of manual work. Designers couldn't create content without developer help. Everything was tightly coupled and hard to change. The game couldn't scale.

**My Role: Backend Developer**

My job was refactoring the architecture and building systems. Not writing stories or designing characters - that's Peter's job as narrative designer. I built the technical foundation he could use to create content.

**What I Built:**

1. **JSON Dialogue System Architecture**
   - Converted hardcoded dialogue to data-driven JSON
   - Built serializable data structures
   - Made it non-coder friendly

2. **Backend Systems**
   - Inbox/mail system for notifications
   - Authorization system for permissions
   - Score tracking and corruption system
   - Scenario loading and management

3. **Developer Tools**
   - Documentation so Peter could work independently
   - Templates for creating scenarios
   - Organized project structure
   - Clear folder organization

4. **Project Management**
   - Version control and Git workflows
   - Repository migration
   - Asset organization
   - Code maintenance

**Working with Peter:**

Peter is the narrative designer. He created the characters, wrote the dialogue, designed the story branches. I built the systems that made his work possible. My JSON system let him add new characters without touching code. That was the goal - make the technical side invisible so he could focus on creativity.

**Technologies:**
- Unity 2022.3.58f1
- C# for backend systems
- JSON for data serialization
- Git/GitHub for version control

---

## 4. Learning Goals

At the start, I wasn't super specific with my goals. Looking back, here's what I wanted to achieve:

### Technical Goals

**Improve Development Skills:**
I wanted to get better at C# and Unity development. Not just following tutorials but actually architecting systems from scratch.

**Handle Technical Problems Confidently:**
Honestly, bugs used to stress me out. I wanted to be more confident debugging and solving problems without panicking.

**Learn New Technologies:**
JSON was new to me, especially integrating it with Unity. I wanted to master that and understand data-driven design.

### Professional Goals

**Deal with Different Workflows:**
School projects are different from real work. I needed to learn professional practices - version control, documentation, team coordination.

**Be Inclusive with My Work:**
I wanted to learn how to work as part of a team, not just solo. Making systems others can use, helping teammates, that kind of stuff.

**Manage Tasks and Professional Life:**
Time management isn't my strongest skill tbh. I needed to get better at estimating time, prioritizing, and balancing multiple tasks.

### Personal Goals

**Gain Real-World Experience:**
Theory is one thing, practice is different. I wanted to see how actual projects work.

**Build Confidence:**
I second-guess myself a lot. Wanted to trust my decisions more.

**Expand Network:**
Meet people in the industry, build connections that might help later.

---

## 5. Activity Logs & Reflections

### Activity 1: JSON Dialogue System - Refactoring the Architecture

**What I Did:**
Completely refactored the dialogue system from hardcoded Unity Inspector values to a JSON-based data-driven architecture.

**Evidence:**
- Git commit: `d1cf91a - JSONTesting` (October 25, 2025)
- Files: `CustomerScenarioDialogue.cs`, `EmmaScenario.json`, Modified `DialogueManager.cs`

**Competencies:** B (Developing and Programming), F (Futures Innovating)

#### STARRT Reflection

**Situation:**

When I started, the game had 3 customers with all their dialogue hardcoded in the Unity Inspector. Want to add a new character? You'd spend hours manually filling in fields in the Inspector, connecting references, testing each branch individually. It was tedious and error-prone.

Peter (our narrative designer) couldn't create content without me because the system required Unity knowledge and manual Inspector work. Every character needed developer intervention. This didn't scale.

**Task:**

Build a JSON-based dialogue system that would:
- Store dialogue data externally in easy-to-edit files
- Let non-coders create scenarios
- Support branching conversations
- Keep all existing features working (ID scanning, items, scoring)
- Make adding characters fast and simple

**Activity:**

First, I analyzed the existing `DialogueManager.cs` to understand what data it needed. I identified three main structures: dialogue lines, player responses, and customer info.

Then I designed new C# classes that could serialize to/from JSON:
- `DialogueLineData` - holds dialogue text, speaker, and possible responses
- `DialogueResponseData` - player choice options
- `CustomerScenarioData` - complete scenario including ID info, items, dialogue tree

I wrote `CustomerScenarioDialogue.cs` with these classes and refactored `DialogueManager.cs` to load JSON files from the Resources folder instead of using Inspector values. This was tricky because I had to maintain backwards compatibility while completely changing how data flowed through the system.

I tested with "EmmaScenario.json" - 92 lines of dialogue with branching paths. If it worked with that complexity, it'd work for anything.

**Results:**

**What Went Well:**
- It worked. JSON loaded, dialogue displayed correctly, branching logic functioned
- Creating new scenarios went from hours to like 30 minutes
- Peter could now create characters without my help
- All existing features (ID scanning, items) still worked
- The architecture was clean and scalable

**What Didn't:**
- JSON errors were hard to debug at first - Unity just crashed
- I underestimated how much documentation Peter would need
- Some edge cases in branching logic needed fixing
- Initial learning curve for team to understand JSON structure

**Reflection:**

This was probably the most important thing I did during the internship. It fundamentally changed how the project worked.

**What I Learned:**
- Data-driven design isn't just a buzzword - it genuinely makes development better
- Good architecture pays off long-term even if it takes longer upfront
- Thinking about who will USE your system is as important as building it
- Refactoring entire systems is scary but sometimes necessary

The hardest part wasn't the technical challenge - it was convincing myself to throw away working code and rebuild it properly. I kept thinking "maybe I should just improve what's there" but that would've been patching a fundamentally limited system.

**Key Moment:**

When Peter created his first character scenario using my system without asking me any questions, that felt amazing. That's when I realized I'd built something actually useful, not just technically correct.

**Transfer:**

For future projects:
- Start with data-driven architecture for content-heavy features
- Build the tool, then test it with real users immediately
- Don't be afraid to refactor when architecture is wrong
- Document while building, not after
- Always think about who'll use the system besides me

---

### Activity 2: Building Backend Systems - Inbox, Authorization, and Scoring

**What I Did:**
Implemented three interconnected backend systems: inbox/mail notifications, authorization permissions, and score tracking. These added gameplay depth without Peter needing to understand the technical implementation.

**Evidence:**
- Git commit: `b190b8d - Inbox system and scoring distribution`
- Files: `InboxUIManager.cs`, `AuthorizationUIManager.cs`, `ScoreManager.cs`
- Scene modifications in GameplayScene

**Competencies:** B (Developing and Programming), F (Futures Innovating)

#### STARRT Reflection

**Situation:**

The JSON dialogue system worked great for conversations, but the gameplay felt flat. Just talking to customers sequentially got repetitive. Remco and Reimer wanted more layers - notifications, permissions, consequences that build up over time.

The challenge was adding these systems without making the JSON structure too complicated for Peter to use.

**Task:**

Build three systems:
- **Inbox:** Show messages/warnings to players
- **Authorization:** Let scenarios check if player has special permissions
- **Score tracking:** Monitor corruption and trigger police scenarios

All needed to integrate with the dialogue system but stay modular.

**Activity:**

**Inbox System:**
Created `InboxUIManager.cs` to handle message display. Messages could be triggered from JSON scenarios at specific dialogue points. I built it so Peter could just add `"triggerInboxMessage": true` in the JSON and the system would handle the rest.

The tricky part was UI state management - messages needed to persist across scenes and show visual indicators for new messages. I used a singleton pattern with DontDestroyOnLoad to maintain state.

**Authorization System:**
Built `AuthorizationUIManager.cs` to track permissions. Scenarios could grant authorizations, and later dialogue choices could check if the player had them. This enabled things like "you need authorization to ask about personal data."

I made it simple for Peter - just reference authorization IDs in JSON, the backend handles checking and granting.

**Score System:**
Refactored `ScoreManager.cs` to track corruption points. Dialogue responses could add points, and the manager would trigger police scenarios at thresholds (50 points = warning, 70 = arrest).

I added debug hotkeys (press 4 to jump to 50 points, 5 for 70 points) so we could test police scenarios without playing through everything.

**Results:**

**What Went Well:**
- All three systems integrated cleanly with existing dialogue
- Peter could use them through simple JSON properties
- Modular design - could modify one system without breaking others
- Added significant gameplay depth
- Testing features (debug keys) made development much faster

**What Didn't:**
- UI timing was tricky - messages sometimes appeared too early or too late
- First version of authorization persistence had bugs across scene loads
- Score system thresholds needed several iterations to balance
- More documentation needed for how to use these features in JSON

**Reflection:**

This taught me about systems integration and API design. I wasn't just building features - I was building tools that Peter would use through my API (the JSON structure).

**Design Thinking:**
- Every feature I added had to answer: "How will Peter use this in JSON?"
- If it was too complicated, I simplified the interface
- Backend complexity is fine if the user-facing API is simple

**Problem-Solving:**
When the authorization system had persistence issues, I initially tried using PlayerPrefs. Terrible idea for complex data. I refactored to a singleton pattern, which taught me about Unity's lifecycle and object persistence.

**User Experience:**
Technical systems need UX thinking too. The inbox visual indicator (red badge on mail icon) made a huge difference in player experience, but I almost skipped it to save time.

**Transfer:**

For future work:
- Design APIs for your teammates, not just yourself
- Simple interface, complex backend is better than exposing complexity
- Test integration early - don't build in isolation
- Debug tools aren't optional - they're essential for development
- Sometimes the right solution requires throwing away the first attempt

---

### Activity 3: Documentation and Making Systems Non-Coder Friendly

**What I Did:**
Created comprehensive documentation so Peter and other team members could use my systems without constantly asking me questions.

**Evidence:**
- Files: `JSON_SCENARIO_GUIDE.md`, `SCENARIO_TEMPLATE.jsonc`, `SCRIPTS_GUIDE.md`
- Updated README with setup instructions

**Competencies:** G (Self-fashioning), E (Organising and Implementing)

#### STARRT Reflection

**Situation:**

I'd built these cool systems, but they were kind of useless if only I understood them. Peter kept asking the same questions: "How do I add a dialogue branch?" "What's the format for triggering inbox messages?" "How do I check authorizations?"

This was a bottleneck. Every time Peter needed something, he had to wait for me. That's not scalable and honestly it was getting tedious answering the same things repeatedly.

**Task:**

Create documentation that would:
- Explain JSON structure clearly
- Show examples of common patterns
- Provide ready-to-use templates
- Let Peter work independently
- Reduce my support burden

**Activity:**

**JSON Scenario Guide:**
Wrote a complete guide explaining every field in the JSON structure. Not just "what" each field does, but "why" you'd use it and examples showing different patterns.

Included sections on:
- Basic dialogue flow
- Branching conversations
- Triggering inbox messages
- Checking authorizations
- Requesting items
- Setting score values
- Common mistakes and how to fix them

**Scenario Template:**
Created `SCENARIO_TEMPLATE.jsonc` (JSON with comments) that Peter could copy as a starting point. Had placeholder values he could replace and comments explaining what everything did.

**Scripts Guide:**
Documented the codebase structure for anyone who needed to modify or understand the backend. Explained which scripts did what and how they connected.

**README Updates:**
Rewrote the README to be actually useful. Before it was just "open project in Unity." After, it explained:
- How to set up the project
- How to test different scenarios
- Debug commands for development
- How the game flow works
- Common issues and fixes

**Results:**

**What Went Well:**
- Peter started creating scenarios completely independently
- My "support tickets" dropped significantly
- New team members could onboard faster
- Fewer bugs from incorrect JSON formatting
- Future me benefited from past me's documentation

**What Didn't:**
- Writing docs felt boring compared to coding
- Docs needed updating when systems changed
- Hard to know how much detail was enough
- Initially I explained things assuming too much knowledge

**Reflection:**

This changed how I think about professional development. Documentation isn't separate from coding - it's part of building usable systems.

**The Realization:**
When Peter successfully created a complex character scenario without asking me a single question, I realized documentation was a force multiplier. My time writing docs paid back multiple times over.

**Communication Skills:**
Writing for others is different from writing for yourself. I had to:
- Assume less background knowledge
- Use concrete examples, not abstract descriptions
- Test documentation with actual users
- Revise based on what confused people

**Professional Responsibility:**
Making my systems accessible isn't optional - it's part of building them properly. Code that only I understand is basically useless to a team.

**Transfer:**

Going forward:
- Write documentation AS I build features, not after
- Test docs with someone who doesn't know the system
- Examples are worth 1000 words of explanation
- Templates lower the barrier to entry significantly
- Good documentation is an investment, not a cost

---

### Activity 4: Project Organization and Asset Management

**What I Did:**
Organized the project structure, particularly score screen assets that were scattered all over. Created logical folder hierarchies and naming conventions.

**Evidence:**
- Git commit: `f4d809f - organizing Folders in Peter-Review`
- Created folder structure: `Peter-Reviews/ScoreScreens/` with character subfolders
- 30+ score screen prefabs organized

**Competencies:** E (Organising and Implementing), G (Self-fashioning)

#### STARRT Reflection

**Situation:**

The project was getting messy. Score screen prefabs were scattered across different folders with inconsistent naming. Peter had created 30+ outcome screens for different character scenarios, but finding the right one was like a treasure hunt.

It wasn't a crisis, but it was annoying and wasted time. "Where's the Fridgy bad outcome screen?" became a regular question.

**Task:**

Organize the chaos:
- Create logical folder structure
- Implement consistent naming conventions
- Make assets easy to find
- Don't break existing references (Unity's GUIDs are fragile)
- Document the organization system

**Activity:**

Created a clear hierarchy:
```
Peter-Reviews/
├── ScoreScreens/
│   ├── Conspiracy/
│   ├── Fridgy/
│   ├── PoliceIntro/
│   ├── PoliceWarning/
│   └── ShadyGuy/
```

Each character got their own folder with their outcome screens. Naming convention: `[Character]_[Outcome].prefab`

**The Process:**
1. Committed current state to Git (safety first)
2. Created new folder structure
3. Moved prefabs carefully (drag in Unity, not file system)
4. Verified references weren't broken
5. Updated any hardcoded paths in scripts
6. Tested each scenario to confirm everything loaded
7. Documented the organization in README

**The Score Screens:**
These existed before I started - they were Peter's work. My job was organizing them and making sure my dialogue system could reference them correctly. I modified the JSON structure to use indices that mapped to specific screens based on outcome type.

**Results:**

**What Went Well:**
- Finding assets became instant instead of frustrating
- Clear structure made adding new screens obvious
- Team members knew exactly where to put things
- Reduced accidental duplicates
- Made the project look professional

**What Didn't:**
- Time-consuming and felt tedious
- Broke some prefab references initially (learned about Unity .meta files)
- Had to fix references in several JSON files
- Underestimated how long this would take

**Reflection:**

This taught me that "boring" work is actually important work.

**Patience and Detail:**
Organization isn't glamorous. It's not as exciting as building new features. But it matters. The time I spent organizing saved everyone time later.

**Professional Standards:**
Good developers don't just write code - they maintain projects. Organization, structure, consistency - these aren't optional extras, they're professional standards.

**The Wake-Up Call:**
Breaking prefab references by moving files incorrectly taught me:
- Unity's asset system is based on GUID in .meta files
- Always move assets inside Unity, not in file explorer
- Git commit before major reorganizations
- Test thoroughly after structural changes

**Transfer:**

For future projects:
- Set up organization from day one, don't wait until it's messy
- Create and document naming conventions early
- Use version control religiously for structural changes
- "Cleanup sprints" should be regular, not one-time
- Good organization enables faster development

---

### Activity 5: Version Control and Repository Management

**What I Did:**
Managed Git workflow for the project, including repository migration from private to public repo while preserving all history.

**Evidence:**
- Repository: https://github.com/DitLabGithub/ProjektKiosk-2026-S1
- 100+ commits
- Successfully migrated repository
- Managed multiple branches

**Competencies:** E (Organising and Implementing), G (Self-fashioning)

#### STARRT Reflection

**Situation:**

The project used Git, but practices were inconsistent. Commit messages were vague, branches weren't managed clearly, and we needed to migrate from a private repo to a new public one.

I became the de facto Git person, handling complex operations and helping teammates when they had issues.

**Task:**

- Maintain clean Git history
- Write useful commit messages
- Manage branches for different features
- Migrate entire repository to new public location
- Help teammates with Git issues

**Activity:**

**Daily Git Work:**
I tried to write commits that actually explained what changed and why. Instead of "fixed stuff" I'd write "organizing Folders in Peter-Review" or "Inbox system and scoring distribution."

Worked primarily on development branches, merged to main when features were stable.

**Repository Migration (January 2026):**
This was nerve-wracking. We needed to move the entire project to a new public GitHub repo while keeping all history.

Steps I followed:
1. Added new repo as remote
2. Pushed all branches and tags
3. Updated origin URL
4. Cleaned up old remote references
5. Verified everything transferred correctly
6. Updated documentation with new URLs

Hit some issues with corrupted references and had to manually fix Git internals, but got it working.

**Results:**

**What Went Well:**
- Clean Git history made tracking changes easy
- Migration preserved all work
- Public repo enabled sharing and portfolio value
- Learned Git at a deeper level
- Helped teammates learn better Git practices

**What Didn't:**
- Early commits weren't as clear as later ones
- Migration broke some local setups initially
- Large binary files (Unity assets) slowed operations
- Merge conflicts happened occasionally

**Reflection:**

Git went from "thing I use to save my work" to "communication tool for the team."

**Communication Through Commits:**
Commit messages aren't just for me - they're explaining to future developers (including future me) what changed and why. This is documentation too.

**Problem-Solving:**
When the migration broke references, I had to dig into Git's internal structure. Learned about refs, packed-refs, remotes, tracking branches. That knowledge will be useful forever.

**Professional Practice:**
Version control isn't just about backing up code. It's about:
- Maintaining project history
- Enabling collaboration
- Communicating changes
- Managing complexity

**Transfer:**

Going forward:
- Write commit messages assuming someone else will read them
- Commit frequently with clear scope
- Branch naming conventions matter
- Document major changes like migrations
- Learn the tools deeply, don't just memorize commands

---

### Activity 6: Collaboration with Narrative Designer

**What I Did:**
Worked with Peter (narrative designer) to enable him to create content using my technical systems.

**Evidence:**
- Peter's work: 7+ character scenarios using my JSON system
- Characters: Robin, Amon, Shady Guy, Fridgy, Conspiracy Guy, Police Officer
- 2000+ lines of dialogue content created by Peter
- Score screen designs by Peter

**Competencies:** G (Self-fashioning), E (Organising and Implementing)

#### STARRT Reflection

**Situation:**

Peter is the narrative designer. He creates characters, writes dialogue, designs the story branches, makes the score screens. I'm the backend developer building systems he uses.

Our success depended on clear collaboration - if my systems were too complicated or poorly documented, Peter couldn't work. If I didn't understand his needs, I'd build the wrong things.

**Task:**

- Build systems Peter could use without coding
- Document everything clearly
- Support him when he had questions
- Understand his creative needs and translate them to technical requirements
- Enable him to work independently

**Activity:**

**Understanding His Workflow:**
I spent time watching how Peter worked and what frustrated him. This informed my system design. For example, when I saw him struggling with complex nested structures, I simplified the JSON format.

**Iterative Improvement:**
Peter would try using my systems, hit issues, tell me what was confusing. I'd either fix the system or improve documentation. This feedback loop made everything better.

**Specific Examples:**

**Character Creation:**
Peter would design a character and their story. I'd ensure my JSON system could express what he wanted. Sometimes this meant adding features (like authorization checks) that he needed.

**Score Screens:**
Peter designed 30+ outcome screens showing different results based on player choices. I organized them and made sure my dialogue system could trigger the right screen based on scenario outcomes.

**Narrative Content:**
All the actual writing - dialogue, character personalities, story branches - that's Peter's work. He used my system to bring his vision to life.

**Results:**

**What Went Well:**
- Peter created complex scenarios independently
- Our systems integrated smoothly - his content, my backend
- Clear division of labor - he focused on creativity, I focused on technical
- Mutual respect for each other's expertise
- Final product shows our collaboration

**What Didn't:**
- Communication gaps sometimes - I'd assume he knew something technical he didn't
- Initial systems needed several iterations based on his feedback
- Sometimes I focused on technical elegance when he just needed something practical

**Reflection:**

This taught me that teamwork means understanding what others need and enabling their success.

**Complementary Skills:**
Peter knows narrative, characters, storytelling. I know systems, architecture, code. Together we built something neither could alone. That's the point of teams.

**Communication:**
Technical people and creative people think differently. I learned to:
- Explain technical concepts in non-technical terms
- Listen to creative needs and translate them to technical requirements
- Not make him learn my world - make my systems work in his world

**Respect:**
Peter's work - the narrative, characters, story - is what players experience. My backend is invisible but enables his work. Both matter. Understanding that balance is important.

**Transfer:**

For future collaboration:
- Understand what teammates need, don't assume
- Design systems for the people who'll use them
- Documentation and communication are as important as code
- Respect different expertise - not everyone should code
- Success means enabling others, not just personal achievement

---

## 6. Competency Demonstrations

### Competency B: Developing and Programming

**Learning Outcome 1:** *The student can construct technical solutions informed by relevant knowledge and theories.*

**How I Demonstrated This:**

I built the JSON dialogue system by applying actual software engineering principles:

- **Data-driven design:** Researched how games like Baldur's Gate 3 and Disco Elysium handle branching narratives
- **Serialization theory:** Understood JSON format and C# serialization mechanisms
- **Unity architecture:** Used Resources.Load, prefab system, component-based design properly
- **Design patterns:** Singleton for managers, factory-ish pattern for creating dialogue instances
- **MVC thinking:** Separated data (JSON) from logic (DialogueManager) from view (UI)

This wasn't me copying a tutorial. I studied how dialogue systems work, understood the principles, and applied them to our specific needs.

**Evidence:** `CustomerScenarioDialogue.cs` showing thoughtful class design, `DialogueManager.cs` refactoring, comprehensive JSON files

**What This Means:**
I can now approach new technical challenges by researching principles and applying them, not just searching for exact solutions online.

---

**Learning Outcome 2:** *The student alters and differentiates technical solutions using identified improvements.*

**How I Demonstrated This:**

My dialogue system went through multiple iterations:

**Version 1 (October):**
Basic JSON loading with simple dialogue chains

**Version 2 (November):**
Added branching logic and response handling

**Version 3 (December):**
Integrated inbox triggers, authorizations, score tracking

**Version 4 (January):**
Performance improvements, error handling, validation

Each version addressed specific problems I identified:
- **Performance:** Changed from loading all scenarios at start to lazy loading
- **Errors:** Added JSON validation so Unity wouldn't just crash on bad data
- **UX:** Improved dialogue transition timing based on playtesting
- **DX:** Created helper methods for common operations

**Evidence:** Git history showing incremental improvements, multiple versions of core files

**What This Means:**
I don't treat initial solutions as final. I identify issues and improve continuously.

---

**Learning Outcome 3:** *The student compares and selects appropriate technical solutions to satisfy complex problems.*

**How I Demonstrated This:**

When deciding on the dialogue system architecture, I evaluated three approaches:

**Option 1 - ScriptableObjects:**
- Pros: Unity-native, Inspector editing
- Cons: Binary format sucks for version control, Git diffs are useless, non-coders can't edit easily
- Verdict: Initially tried this in September but didn't commit it because of these issues

**Option 2 - XML:**
- Pros: Structured, validated, well-supported
- Cons: Verbose as hell, harder to read/write than JSON
- Verdict: Too heavyweight for our needs

**Option 3 - JSON (Chose This):**
- Pros: Human-readable, easy to edit, great version control, widely supported
- Cons: No built-in validation, manual serialization work
- Verdict: Best balance of readability and functionality

I picked JSON because:
- Peter could edit text files directly
- Git showed actual changes in dialogue, not binary differences
- External tools could validate if needed
- Industry standard, well-documented
- Simple enough for non-programmers, powerful enough for complex branching

**Evidence:** `JSON_SCENARIO_GUIDE.md` explaining architecture decisions, commit history showing exploration

**What This Means:**
I can evaluate trade-offs and make informed decisions based on actual project needs, not just picking what's trendy or what I'm comfortable with.

---

### Competency E: Organising and Implementing

**Learning Outcome 1:** *The student can plan, implement, monitor and manage process-based projects in a complex but structured context.*

**How I Demonstrated This:**

I managed the development of multiple interconnected systems over 20 weeks:

**Planning:**
- Broke down "refactor game architecture" into concrete tasks
- Prioritized based on dependencies (JSON system first, then features that use it)
- Coordinated with stakeholders on what features mattered most

**Implementation:**
- Built systems incrementally - get core working, then add features
- Maintained working build throughout (never broke main branch for long)
- Tested each component before moving to next

**Monitoring:**
- Regular check-ins with Remco and Reimer
- Tracked what was done vs what remained
- Identified blockers early (like when I realized Peter couldn't use the system without docs)

**Management:**
- Balanced multiple concurrent tasks (dialogue system + inbox + organization)
- Communicated progress and issues
- Adjusted plans when reality didn't match expectations (like underestimating documentation time)

**Evidence:** Git commit timeline showing systematic development, meeting notes, project progression

**What This Means:**
I can manage complex technical projects, not just write code. Planning, prioritizing, communicating - these are part of professional development.

---

**Learning Outcome 2:** *The student can compare and choose appropriate channels and business models for their solution.*

**How I Demonstrated This:**

**Distribution Channel:**
Contributed to decision to build for WebGL. Trade-offs:
- Loses some performance vs downloadable builds
- But gains massive accessibility - anyone can play in browser
- Aligns with DIT-Lab's goal of making prototypes accessible

**Documentation Channel:**
- Chose Markdown for docs (GitHub-friendly, version control works)
- Used in-code comments for implementation details
- Separate guides for different audiences (Peter vs future developers)

**Communication:**
- Git for code/changes
- Documentation for asynchronous knowledge transfer
- Direct communication for complex decisions

**Evidence:** README.md structure, WebGL build configuration, documentation system

**What This Means:**
I understand technical decisions have practical implications beyond just "does it work."

---

**Learning Outcome 3:** *The student discusses and justifies the added value of a chosen concept or solution in a complex context utilising appropriate means of communication.*

**How I Demonstrated This:**

I regularly explained and justified technical decisions:

**JSON System to Stakeholders:**
- Explained how it saves time in content creation
- Demonstrated with concrete example (hours to minutes)
- Showed how it enabled Peter to work independently
- Presented through working prototype, not just theory

**Organization to Team:**
- Articulated why scattered files waste time
- Demonstrated improved workflow
- Documented principles so others understood the system

**Documentation to Remco/Reimer:**
- Quantified reduction in support questions
- Showed Peter's increased independence
- Explained long-term maintainability benefits

**Different Communication for Different Audiences:**
- Technical details for developers (in SCRIPTS_GUIDE)
- User-focused instructions for Peter (in JSON_SCENARIO_GUIDE)
- High-level overview for stakeholders (in presentations)

**Evidence:** Documentation explaining decisions, commit messages with rationale, stakeholder meetings

**What This Means:**
I can explain technical work in ways that make sense to different audiences, not just other programmers.

---

### Competency F: Futures Innovating

**Learning Outcome 1:** *The student experiments with new technological trends and models a realisable solution.*

**How I Demonstrated This:**

I experimented with data-driven game design, which is becoming standard in the industry:

**Research Phase:**
- Studied how modern narrative games use JSON/data-driven systems
- Looked at Baldur's Gate 3's dialogue system approach
- Researched Unity serialization best practices
- Explored JSON Schema for validation

**Experimentation:**
- Prototyped JSON loading with simple test case first
- Tried different serialization approaches (JsonUtility vs Json.NET)
- Tested various data structures before settling on final design
- Experimented with how much to put in JSON vs code

**Realisable Solution:**
- Created working system that solved real problems
- Balanced flexibility with simplicity
- Made it accessible to non-technical users
- Ensured it met project requirements

**Evidence:** JSONTesting commit showing initial experiments, final working system, documentation

**What This Means:**
I can explore new approaches while staying pragmatic. Not chasing trends, but adopting useful techniques.

---

**Learning Outcome 2:** *The student can experiment with innovative concepts to address complex or complicated situations.*

**How I Demonstrated This:**

The inbox system was an innovative solution to a design problem:

**Problem:**
- Gameplay felt linear - just serve customers sequentially
- Needed additional depth without making dialogue too complex
- Wanted meta-layer that affected decisions

**Innovative Concept:**
- Messages arriving between encounters
- Authorizations affecting available dialogue choices
- Building narrative through asynchronous communication

**Experimentation:**
- Tried different UI approaches (full screen vs notification)
- Tested when messages should trigger (during dialogue vs after)
- Experimented with how authorizations integrate with choices
- Iterated based on playtest feedback

**Results:**
- Added gameplay layer that felt natural
- Enhanced player agency
- Increased replay value
- Pattern applicable to future scenarios

**Evidence:** `InboxUIManager.cs`, `AuthorizationUIManager.cs`, integrated gameplay showing the system working

**What This Means:**
I can think creatively about technical solutions to design problems, not just implement what's asked.

---

**Learning Outcome 3:** *The student can experiment with different solutions and reflect upon their impacts and consequences.*

**How I Demonstrated This:**

Throughout development, I tried alternatives and learned from them:

**Dialogue State Management:**
- **Attempt 1:** Global state dictionary - simple but error-prone
- **Attempt 2:** Per-scenario state - better encapsulation but harder to track
- **Final:** Hybrid approach balancing both needs
- **Impact:** Global made debugging easier, encapsulation prevented conflicts

**Message Persistence:**
- **Attempt 1:** PlayerPrefs - seemed simple
- **Attempt 2:** Singleton with DontDestroyOnLoad
- **Final:** Singleton approach
- **Impact:** PlayerPrefs failed with complex data, Singleton properly handles Unity lifecycle

**ScriptableObjects vs JSON:**
- **Attempt 1:** ScriptableObjects in September (never committed)
- **Final:** JSON system
- **Impact:** ScriptableObjects easier initially but killed version control and team collaboration

**Evidence:** Multiple commit versions showing different approaches, documentation discussing trade-offs

**What This Means:**
I don't just accept the first working solution. I evaluate alternatives and understand consequences.

---

### Competency G: Self-fashioning

**Learning Outcome 1:** *The student knows their own strengths and weaknesses, can formulate complex learning goals, reflects on and takes responsibility for managing their own learning process.*

**How I Demonstrated This:**

**Strengths I Identified:**
- Logical thinking and system design
- Problem-solving persistence
- Written communication
- Learning new technologies quickly

**Weaknesses I Identified:**
- Time estimation (consistently underestimated)
- Asking for help (initially too stubborn)
- Sometimes over-engineering when simple would work
- Confidence in my decisions

**How Goals Evolved:**

**Start (September):**
"Improve development skills" - too vague

**Mid-way (November):**
"Master JSON serialization in Unity, create functional dialogue system" - specific and measurable

**End (January):**
Achieved: JSON system works, 7+ scenarios running on it, documentation complete, team can use it independently

**Self-Management Examples:**

**The Debugging Struggle:**
Early on, I spent 4 hours stuck on a dialogue branching bug. Refused to ask for help because I felt stupid. Finally asked a teammate, solved in 20 minutes.

**Lesson:** Asking for help isn't weakness. Now I try independently for reasonable time, then ask with specific questions about what I've tried.

**Time Estimation:**
First estimate for JSON system: "Maybe a week?" Reality: 3 weeks. I learned to multiply my instinctive estimates by 2-3x.

**Evidence:** Learning goals documented at start, reflection throughout, improved estimates over time

**What This Means:**
I actively manage my own learning instead of passively waiting to be taught.

---

**Learning Outcome 2:** *The student acts and performs within a team, valuing the team's diversity and facilitating contributions of team members.*

**How I Demonstrated This:**

**Team Context:**
- Peter (narrative designer) - creative side
- Other developers - different technical specialties
- Stakeholders (Remco & Reimer) - project direction

**Valuing Diversity:**
- Recognized Peter's narrative skills far exceed mine
- Understood I shouldn't try to do his job
- Appreciated that his creative thinking balanced my technical thinking
- Respected that different people bring different value

**Facilitating Contributions:**

**For Peter:**
- Built systems he could use without coding
- Created documentation so he could work independently
- Made JSON structure as simple as possible
- Provided support when he needed it

**For Team:**
- Organized project so others could find things
- Wrote clear commit messages
- Documented code for future developers
- Shared knowledge through guides

**Specific Example:**
When Peter wanted to add a character, previously he'd wait for me. After documentation:
1. He'd copy SCENARIO_TEMPLATE.jsonc
2. Write his dialogue following the guide
3. Test it himself
4. Only asked questions if he hit edge cases

This empowered him and freed my time for complex technical work.

**Evidence:** Documentation created for team, Peter's successful independent work, organized project structure

**What This Means:**
I understand professional development includes enabling others, not just personal technical achievement.

---

**Learning Outcome 3:** *The student builds their own network, brings people in contact with each other and stimulates information exchange.*

**How I Demonstrated This:**

**Network Built:**
- Remco and Reimer - professional relationships continuing beyond internship
- DIT-Lab team members - potential future collaborators
- CMGT community - shared experiences
- GitHub community - public repo creates professional visibility

**Information Exchange:**

**Within Project:**
- Documentation as central knowledge base
- Connected Peter with technical resources
- Shared learnings from experiments
- Made information accessible instead of siloed in my head

**Broader Community:**
- Public repository for educational purposes
- Preparing to share internship experience with younger CMGT students
- Documented processes others can learn from

**Specific Example:**
When a teammate struggled with Unity serialization, I:
1. Remembered another developer solved similar problem
2. Connected them directly
3. Documented the solution for future reference
4. Created knowledge ripple effect

**Evidence:** Public GitHub repository, documentation accessible to community, connections maintained post-internship

**What This Means:**
Networking isn't just collecting contacts - it's creating value through connections and sharing knowledge.

---

## 7. Learning Outcomes & Reflection

### Did I Achieve My Goals?

**Technical Development:**

✓ **Improved development skills** - Yeah, significantly. Went from following tutorials to architecting complete systems. Feel confident tackling complex technical challenges now.

✓ **Handle technical problems confidently** - This one improved a lot. Bugs don't freak me out anymore. I have a systematic approach to debugging instead of panicking.

✓ **Learn new technologies** - JSON serialization in Unity is now comfortable. Also learned deeper Git, Unity lifecycle stuff, design patterns. More importantly, I learned HOW to learn new tech.

**Professional Development:**

✓ **Deal with different workflows** - Adapted to professional practices. Version control, documentation, code review, stakeholder communication - these all make sense now.

✓ **Be inclusive with work** - Built systems others can use. Created documentation. Learned that good development includes enabling teammates.

✓ **Manage tasks and professional life** - Still not perfect at time estimation but way better. Learned to prioritize, balance multiple tasks, communicate progress.

**Personal Growth:**

✓ **Gain real-world experience** - Definitely. Understand the difference between school projects and actual professional work now.

✓ **Build confidence** - This improved significantly. Trust my technical decisions more. Still learning but don't second-guess everything constantly.

✓ **Expand network** - Met people at DIT-Lab, built relationships. Understand networking as ongoing practice, not one-time thing.

### What Surprised Me

**Documentation's Impact:**
Didn't expect writing guides to be so valuable. Thought it was just boring requirement. Turned out documentation multiplies effectiveness - enables others while freeing my time.

**Iterative Nature:**
School projects have clear endpoints. Professional development is continuous refinement. The dialogue system went through like 4 major versions, each better.

**Non-Technical Skills:**
Expected technical excellence to be enough. Wrong. Communication, organization, collaboration matter just as much as coding ability.

**Mistakes Are Normal:**
Every bug felt like personal failure at first. Learned that professional development includes bugs, mistakes, and constant refinement. That's just how it works.

### What Challenged Me

**Time Estimation:**
Consistently underestimated complex tasks. "This should take a day" = actually takes a week. Getting better but still learning.

**Asking for Help:**
Struggled with feeling stupid when I didn't know something. Learned asking good questions is a professional skill, not a weakness.

**Perfectionism:**
Sometimes focused too much on technical elegance when practical/working would've been fine. Learning when "good enough" is appropriate.

**Communication:**
Explaining technical concepts to non-technical people was harder than expected. Had to learn different communication styles.

### Key Learnings

**1. Systems Thinking:**
Good architecture isn't clever code - it's systems others can understand and use. If only you can work with it, it's not good.

**2. Enable Others:**
Your value as developer includes how much you enable teammates. Documentation, clear systems, good organization - these multiply team effectiveness.

**3. Collaboration > Solo Work:**
Working alone limits you to your capacity. Building systems others can use multiplies what's possible.

**4. Process Matters:**
Version control, testing, documentation, organization - these aren't obstacles, they're enablers of sustainable development.

**5. Continuous Learning:**
There's no "becoming" a professional - it's ongoing process. Always more to learn, always ways to improve.

### Comparing Expectations vs Reality

**Expected:** Internship would be mostly writing code
**Reality:** Professional development is holistic - code quality, organization, documentation, communication, collaboration all matter equally

**Expected:** Learn specific technologies and frameworks
**Reality:** Most important learning was about practices, collaboration, and professional thinking

**Expected:** Professional developers always know the right answer
**Reality:** Professional developers evaluate options, make informed decisions with incomplete information, iterate based on results

### How This Changed Me

**Technical Skills:**
- From tutorial follower to system architect
- From random bug fixing to systematic debugging
- From writing code to designing systems

**Professional Skills:**
- From individual contributor to team enabler
- From implicit knowledge to explicit documentation
- From reactive to proactive communication

**Mindset:**
- From fearing mistakes to seeing them as learning
- From seeking perfection to embracing iteration
- From individual achievement to collaborative success

**Confidence:**
- Trust technical decisions more
- Ask for help without hesitation
- Communicate with stakeholders effectively
- Recognize my value as developer

### What I'd Do Differently

**Start Documentation Earlier:**
I wrote docs after confusion happened. Next time, document while developing.

**Smaller Commits:**
Some commits bundled too much. More frequent commits with clearer scope would be better.

**More Testing:**
Relied heavily on manual testing. Should invest in automated tests for critical systems.

**Better Time Tracking:**
Would track actual time spent to improve estimation accuracy.

**More Frequent Stakeholder Demos:**
Sometimes worked for weeks before showing progress. More frequent demos would've been valuable for feedback.

---

## 8. Future Aspirations

### Short-Term (Next 6 Months)

**Portfolio:**
- Create portfolio website showcasing Projekt Kiosk
- Document case studies for major systems
- Include code samples and architectural explanations
- Video demonstrations of features

**Skills:**
- Deepen Unity expertise (optimization, advanced features)
- Learn more design patterns and architecture
- Study game design theory more systematically
- Explore AI/ML in games

**Network:**
- Attend game dev meetups in Netherlands
- Participate in game jams
- Contribute to open-source projects
- Connect with CMGT alumni in industry

**Job Search:**
- Apply to junior developer positions
- Target studios working on narrative-driven games
- Prepare for technical interviews

### Medium-Term (1-2 Years)

**Professional Experience:**
- Secure junior developer position
- Work on shipped commercial game
- Learn production pipelines
- Build reputation for quality work

**Specialization:**
- Focus on gameplay programming or tools development
- Develop expertise in narrative systems
- Strong foundation in one engine (Unity or Unreal)

**Continued Learning:**
- Advanced game development courses
- Deeper CS fundamentals
- Adjacent skills (DevOps, CI/CD for games)
- Stay current with industry

### Long-Term (3-5 Years)

**Career:**
- Progress to mid/senior developer role
- Lead technical aspects of projects
- Mentor junior developers
- Contribute to game dev community through talks or writing

**Personal Projects:**
- Develop and release independent game
- Build open-source tools for developers
- Create educational content about game dev

**Impact:**
- Known for technical excellence and collaboration
- Help improve industry practices
- Contribute to making game development more accessible

### How Internship Supports This

**Foundation:**
- Professional work experience on real project
- Portfolio piece demonstrating capabilities
- Network connections at DIT-Lab
- Understanding of professional practices

**Skills:**
- System architecture and design
- Collaboration and documentation
- Version control and workflows
- Stakeholder communication

**Confidence:**
- Trust in technical abilities
- Comfort with professional environments
- Understanding what professional development actually means
- Recognition that I can contribute value

**Clarity:**
- Confirmed interest in game development career
- Identified gameplay/systems programming as potential specialization
- Understood importance of non-technical skills
- Recognized value of continuous learning

---

## 9. Appendices

### Appendix A: Evidence Repository

**GitHub Repository:**
https://github.com/DitLabGithub/ProjektKiosk-2026-S1

**Key Commits:**
- `d1cf91a` - JSONTesting (October 25, 2025) - Initial JSON system
- `6f724d1` - ShadyGuy implemented (Peter's content using my system)
- `3afbf9f` - Fridgy (Peter's content)
- `9c72cf2` - PoliceOfficer&ScoringSystem
- `0a37d43` - Conspiracy Guy is done (Peter's content)
- `b190b8d` - Inbox system and scoring distribution
- `f4d809f` - organizing Folders in Peter-Review
- `4c122ff` - ReviewPanels
- `7994227` - IDInfoPanel

**Documentation I Created:**
- `JSON_SCENARIO_GUIDE.md`
- `SCENARIO_TEMPLATE.jsonc`
- `SCRIPTS_GUIDE.md`
- `README.md` (updated extensively)

**Key Backend Scripts:**
- `CustomerScenarioDialogue.cs` - JSON data structures
- `DialogueManager.cs` - Core dialogue system (heavily modified)
- `InboxUIManager.cs` - Message system
- `AuthorizationUIManager.cs` - Authorization system
- `ScoreManager.cs` - Score tracking
- `ScenarioManager.cs` - Scenario loading

**Peter's Content (Using My Systems):**
- 7+ character scenarios in JSON format
- Characters: Robin, Amon, Shady Guy, Fridgy, Conspiracy Guy, Police Officer (3 scenarios)
- 2000+ lines of dialogue content
- 30+ score screen designs

### Appendix B: Project Statistics

**Timeline:**
- Start: September 2, 2025
- End: January 24, 2026
- Duration: ~20 weeks

**Development Metrics:**
- 100+ commits to repository
- ~5,000+ lines of C# backend code
- ~2,000+ lines of dialogue content (by Peter)
- ~3,000+ words of technical documentation
- 7+ character scenarios implemented
- 30+ score screens organized and integrated
- 5+ active branches managed

**My Code Contributions:**
- New scripts: 10+
- Modified scripts: 15+
- JSON structure designed
- Prefabs created for systems: 10+
- Scenes modified: 2 (GameplayScene, TutorialScene)

### Appendix C: Technology Stack

**Development Environment:**
- Unity 2022.3.58f1
- Visual Studio 2022 / VS Code
- Git/GitHub
- Windows 10

**Languages:**
- C# (primary)
- JSON (data)
- Markdown (documentation)

**Unity Systems:**
- Unity UI (Canvas, TextMeshPro)
- Resources system
- Prefab system
- Scene management
- Serialization system

**Practices:**
- Git branching and merging
- Code documentation
- Version control best practices
- Agile/iterative development

### Appendix D: Competency Mapping

| Activity | Comp B | Comp E | Comp F | Comp G |
|----------|--------|--------|--------|--------|
| JSON Architecture | ✓✓✓ | ✓ | ✓✓ | ✓ |
| Backend Systems | ✓✓✓ | ✓ | ✓✓ | - |
| Documentation | ✓ | ✓✓ | - | ✓✓✓ |
| Organization | ✓ | ✓✓✓ | - | ✓✓ |
| Version Control | ✓ | ✓✓ | - | ✓✓ |
| Collaboration | - | ✓✓ | - | ✓✓✓ |

Legend:
- ✓✓✓ = Primary demonstration
- ✓✓ = Strong demonstration
- ✓ = Supporting demonstration

### Appendix E: Learning Resources

**Resources Used:**

**Unity:**
- Unity Manual (Serialization, UI, Resources)
- Unity Scripting Reference
- Unity Forums

**C# and Programming:**
- Microsoft C# Documentation
- JSON.NET documentation
- Design Patterns resources

**Game Development:**
- Game Programming Patterns by Robert Nystrom
- GDC talks on dialogue systems
- Postmortems of narrative games

**Professional Skills:**
- Git documentation
- Technical writing guides
- Agile methodology resources

### Appendix F: 360-Degree Feedback

**[To be added after receiving feedback from Remco and Reimer next week]**

This section will include:
- Remco's feedback
- Reimer's feedback
- Team member feedback

Topics to be covered:
- Technical competence
- Collaboration and teamwork
- Professional attitude
- Communication skills
- Project contribution

### Appendix G: Activity Timeline

| Weeks | Key Activities | Competencies |
|-------|---------------|--------------|
| 1-4 | JSON system architecture and initial implementation | B, F |
| 5-8 | Backend systems (inbox, authorization, scoring) | B, F |
| 9-12 | System refinement, Peter creates scenarios using my systems | B, E, G |
| 13-16 | Additional features, continued Peter collaboration | B, E |
| 17-18 | Project organization, asset management | E, G |
| 19-20 | Documentation, repository migration | E, G |

### Appendix H: Glossary

**Technical Terms:**

- **JSON:** JavaScript Object Notation - human-readable data format
- **Serialization:** Converting data structures to storable format
- **Prefab:** Unity's reusable GameObject template
- **Branching Narrative:** Story with multiple paths based on choices
- **Repository:** Version-controlled project storage
- **Commit:** Saved snapshot in version control
- **Backend:** Systems and logic that run behind the scenes

**Project Terms:**

- **Scenario:** Complete character interaction (created by Peter using my system)
- **Score Screen:** Visual feedback after interaction (designed by Peter, organized by me)
- **Authorization:** Permission system affecting dialogue choices
- **Inbox:** Message/notification system
- **Dialogue System:** Backend architecture for conversations
- **Data-Driven:** Content stored in data files rather than hardcoded

---

## Conclusion

This internship taught me that professional development is more than writing code - it's about building systems others can use, enabling teammates, documenting work, and continuously improving.

**Main Takeaways:**

1. **Backend architecture matters** - Good systems enable content creation
2. **Enable others** - Your value includes how much you help teammates succeed
3. **Documentation is development** - Not separate work, part of building usable systems
4. **Collaboration multiplies impact** - Peter's content + my systems = better result than either alone
5. **Continuous improvement** - Professional growth never stops

**Personal Growth:**

I entered as a student comfortable with tutorials. I'm leaving as a developer who can architect systems, work in teams, and think about long-term maintainability. The confidence boost is huge tbh.

**Thanks To:**

- **DIT-Lab** for the opportunity
- **Remco and Reimer** for trust and guidance
- **Peter** for teaching me about collaboration and creative/technical partnership
- **Team members** for support
- **CMGT programme** for preparation

**What's Next:**

This confirmed I want to work in game development, specifically backend/systems programming. I'm ready to take what I learned here into a professional role and keep growing.

The journey from student to professional developer has started.

---

**Salman Saab**
CMGT Student #461950
January 2026

---

**Document Info:**
- Version: 2.0 (Corrected)
- Date: January 2026
- Format: CMGT Internship Development Portfolio
- Word Count: ~10,000 words
- Status: Ready for submission (pending 360-degree feedback)
- Repository: https://github.com/DitLabGithub/ProjektKiosk-2026-S1
