### &nbsp;                                   **OddOneOut (ReactionLab) - Unity Internship Assignment**



A reaction-based puzzle game built in Unity where players must quickly identify the outlier in a 3x3 grid against a ticking clock.







### \*\*\* How to Run/Play \*\*\* ------



###### Play Online (WebGL): You can play the latest build directly in your browser here: **https://anmol-agarwal.itch.io/reactionlabinternship-assi**







### \*\*\* Controls \*\*\* ------



Use your Mouse (Left Click) to select the tile you believe is the odd one out.



Gameplay Loop: You start with 30 seconds on the clock. Your goal is to clear as many rounds as possible.



Correct Click: Instantly loads the next round.



Incorrect Click: Penalizes you by subtracting 10 seconds from the timer.



Game Over: The game ends when the timer reaches zero or the manual "Stop Game" button is pressed.







### \*\*\* Implemented Rules \& Features \*\*\* ------



3x3 Grid Generation: Every round spawns a fresh 3x3 grid of interactive tiles.



Randomized Rule Variation: Each round randomly selects between two distinct rules to determine the "odd" tile:



Color Rule: The odd tile is distinctively red, while normal tiles are white.



Rotation Rule: Normal tiles share one random angle, while the odd tile is set to a different random angle (guaranteed to be visually distinct).



Fading Distractors Mechanic: To aid visual processing, the "normal" tiles fade to transparent over 1.0 second, leaving only the odd one visible if the player waits too long.



Time Attack Mode: The game has been pivoted from a simple score counter to a high-pressure survival mode against a countdown timer.



Complete Game Flow UI: Includes a Main HUD with Score and Timer, a "Stop Game" button, and a detailed Game Over screen displaying Final Score, Rounds Played, and Accuracy Percentage.







### \*\*\* Adaptive Difficulty System \*\*\* ------



The game employs an adaptive system designed to reward consistent high performance in the Time Attack mode.



The system tracks a moving average of the scores achieved in the last 5 rounds.



Scores are calculated based on reaction speed (faster clicks = higher scores).



If the player's 5-round running average exceeds a high threshold (indicating they are finding tiles instantly), the game recognizes a "High Performance Streak" and instantly awards a +10 second time bonus.



This transforms the difficulty from simply things getting faster, to an endurance challenge where maintaining focus rewards you with more survival time.







### \*\*\* 5 Things I Would Improve (With More Time) \*\*\* -----



Expanded Rule Variations: I would implement further variations beyond color and rotation, such as differing shapes (squares vs circles), different sprite icons, or scale differences to greatly increase variety.



Audio Feedback: The game is currently silent. Adding sound effects for correct/incorrect clicks, a ticking clock ambiance that speeds up as time runs out, and UI sounds would significantly enhance game feel.



Visual "Juice": Adding particle explosion effects upon clicking the correct tile and a subtle screen-shake effect upon clicking the wrong tile would make interactions feel more satisfying and impactful.



Online Leaderboards: Integrating a lightweight backend (like LootLocker or PlayFab) to track global high scores would add a competitive element.



Mobile Optimization: While the WebGL build works, I would specifically adjust the UI canvas scaling and test touch input responsiveness to ensure a seamless experience on mobile devices.

