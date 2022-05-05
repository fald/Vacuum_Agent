# Reinforcement Learning with Unity
**Tags:** #reinforcement-learning #unity #machine-learning #game-dev

**Description:** This project used ML-Agents in Unity! Have an agent discover, then suck up bits of debris around the arena floor.
From the 3 main fields of machine learning ([[Supervised_Learning]], [[Unsupervised_Learning]], [[Reinforcement_Learning]]), we will work with RL.

**Tutorial source:** https://www.youtube.com/watch?v=r_D-sBEXWlY
Presented by Max Hager with FreeCodeCamp

**Files/Repo:** https://github.com/fald/Vacuum_Agent.git

## Next Steps
- [x] Add a little movement animation :3
- [x] Test it with more debris
	- [ ] Update code to easily add arbitrary amount of debris, instead of relying on duplicating it in the inspector X times, just fill in a # field.
- [ ] Test it with time reducing reward
- [ ] Test it in PvP
- [ ] Test it with walls
- [ ] Non-pickup blocks
	- [ ] Minus points
	- [ ] No interaction
	- [ ] Absorb, but no points?

## Theory
### Building Blocks
- An agent
- An environment in which the agent acts
- The agent acts based on environment observations, $O_t$ 
- $t$ stands for time steps, which are the intervals within which the agent acts.
- Based on $O_t$, the agent performs actions, $A_t$
- Through those actions, the agent gets a reward, $R_t$
- The collective actions, rewards, and observations are called the history:

> $H_t = A_t, O_t, R_t$

- From these interactions, we can derive the state at a certain time as a function of history

> $S_t = f(H_t)$



### [[Markov Decision Processes]]
- The MDP is a tuple containing:
	1. The set space of all states from the initial state to the current state:

	> $S = (S_1, S_2, ..., S_t)$  

	2. The action space within the set of all actions:
	
	> $A = (A_1, ..., A_t)$
	
	3. The transition function, $P$, which returns at a given state and action, the likelihood of landing at a certain outcome state, $S_{t+1}$
	
	> $P = \mathbb P(S_{t+1} | S_t, A_t)$	
	
	4.  The reward function, $R$, which determines the received reward after transitioning from state $S_t$ with action $A_t$ to $S_{t+1}$
	
	> $R = (S_t, A_t, S_{t+1})$


### Policy
- The policy needed for PPO is just the probability that an action will be taken given some state.
> $\pi = \mathbb P(A_t | S_t)$


### [[Proximal Policy Optimization]]
- The part where it gets complicated! Using:
	- $L^{CLIP}(\theta)$ : The policy loss, using $\theta$ which is the 'debates' of a neural network
	- $\hat E_t$ : The weighted average of all possible outcomes from time step, $t$
	- $r_t(\theta) = {\pi_\theta(a_t | S_t)} / {\pi_{\theta_{old}}(a_t | S_t)}$  : The ratio from the current time step, $t$ between the current policy and the periods (have fun reading it)
	- $\hat A_t$ : The advantage for choosing a certain action from a certain state.
	- $\epsilon$ : usually set to $0.2$ for reasons

With these elements, we have our algorithm:

> $\LARGE L^{CLIP}(\theta) = \hat E_t[min(r_t(\theta)\hat A_t, clip(r_t(\theta), 1 - \epsilon, 1 + \epsilon)\hat A_t)]$

If the probability of the minimum is positive, the probability of taking the action increases.

The average of all results will give us the final policy loss.


## Installation
**Requirements**:
- Unity
	- `ML-Agents`
		- Able to get directly from Unity's package manager - though project settings need to be changed to allow for preview packages if you want to do anything worth doing (Verified version is 1.0.8 or something, need 1.4+, at time of writing, current version is 2.x)
- Python
	- `pytorch`
	- `mlagents`


## Single Debris - Game #1
### Setup
- Ensure your script is using `Unity.MLAgents` and `Unity.MLAgents.Actuators`.
- Create a game object field to act as the goal. Remember to tag it, and set it as a trigger.
- Within the inspector, apply the script to the vacuum agent, and ensure the debris is set as the goal.
- You will note that, since we're using `MLAgents` things, the script has other items attached to it - Max Steps, notably, which we set to 4000. This is the number of steps per episode that an agent takes.
- Also add the `Decision Requester`, `RigidBody` (for movement), and `Ray Perception Sensor 3D` component's to your agent.
- Work on the behavior parameters:
	- Set a behavior name, which'll come in useful later.
	- Set the space size, which is the length of vector observations for the agent.
	- Set the stacked vectors, which act as the memory (past and future predictions) - this can just be 1 in this case.
	- Set the actions - we don't need continuous actions, but 2 discrete ones. We'll be using tank controls, so the first branch is forward, backward, or no movement. The second branch is rotation left, right, or none. So 2 discrete branches of size 3.
	- Set the behavior type - heuristic-only for now, which is manual controls. Inference is for if we have a brain/model to get input from, and default is for training.
- Work on the perception sensor:
	- Add a detectable tag - whatever you named the goal variable in the script. So...'goal.'
	- Increase the rays per direction. 24 seems decent.
	- Ray degrees - for this case, we'll give the 'bot the same vision we have, so 180$^o$
	- Sphere cast radius is just the sphere-cast size. Small is fine for this project, 0.1
	- Make sure that the offsets can reach the debris! I used -1, -0.6 respectively. If the goal is in-line with a ray, and that ray sees the goal, the ray will change color to a red.
- That's all the setup before the code!
- Honestly, my guy Max is pretty bad at this, wonder how well I can follow along ☹

### Code
- We need several things:
	- A general counter.
	- The movement and turn speeds.
	- Vectors to hold our position as well as the debris' position (which starts off as our position and gets updated later)
	- our `rigidbody`
- In the initialize function, we set the `startPosition` and `debrisPosition` to both be `transform.position`, then use `GetComponent` to grab our `rigidbody`.
- We also need an `OnEpisodeBegin` function that gets called whenever our steps are at 0 - in here, we:
	- reset the state of the goal to be active (They get deactivated on collection).
	- Reset our own position.
	- Reset the height of the debris.
	- Move the goal's position to a new random location:  
		- `goal.transform.position = debrisPosition + (Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * 5f)`
- If controlling heuristically (and we are), we also need to override the `Heuristic(in ActionBuffers actionsOut)` method. 
	- We'll need the vertical/horizontal axes as input
	- Also `ActionSegment<int>` (int because of our discrete branches) to define the actions we can take.
	- Then just define the segments, `action[0] = vertical >= 0 ? vertical : 2`, and similar for the horizontal.
- Next is the `OnActionsReceived` method, with an argument that holds the keyboard inputs.
	- As a first step, we want to determine if the agent has strayed too far from the target object - if so, add a negative reward and end the episode.
	- Next, we assign horizontal/vertical values from the actions again:
		- `float vertical = actions.DiscreteActions[0] <= 1 ? ... : -1;`
	- And if the horizontal is not 0 (i.e., we're turning), actually rotate the agent.
	- Do similar for the forward/vertical movement (no need to check for 0)
- Next is the `OnTriggerEnter` method - what happens when you hit the dirt?
	- Well, there's only one piece, so make sure the tag fits, add a reward, disable the object, up your counter. If there were more than 1 goal, we might also want to decide whether we should end the episode.

### Editor Changes
- Now we have a user-controlled pill that basically works, we will want to pile the ground, goal, and agent into an empty parent object, to turn it into a prefab.
- We should then duplicate this prefab, so we have many arenas to learn on simultaneously. I went for 9 copies.
- We want to actually have it learn, though, so also change the (prefab's!) `Behavior Parameters` to reflect this. `Default` is the training option.

### Config
- Using the PPO config template from the GitHub repo, save something resembling it in a config folder. This handles things like epoch lengths, network dimensions, and so on.

### Training and Beyond
- Now we actually run the training!
> `mlagents-learn config/SingleDebris.yml --run-id=SingleDebrisTrain`

- The ID is important; if you wanted to train more than once, you'll want unique IDs
- Once you run that command, you should be prompted to start playing the game in the editor. So...go do that.
- When done, barring any errors, you should have your training results in an `onnx` file.
- This file is what our brain is - done training, this is production, baby. This goes under `Model` in the `Behavior Parameters` of the agent.
- Now that we're using a brain, we use the `Behavior Typ: Inference Only` (as opposed to `Default`, for training, or `Heuristic Only`, for user control).


## Multiple Debris - Game #2
### Setup
- We can start off by cloning the first game's scene for the basics.
- We can also clone the script, remembering to change the script's name within the file (not just the editor).
- Changes to the agent:
	- Remove the `Decision Requester` and then the `Single Debris` components, to replace them with the new `Multiple Debris` and a new `Decision Requester` component.
	- Modify the `Behavior Parameters : Name` to `Multiple Debris`.
	- Remove the old brain from `Behavior Parameters : Model` and set the `Behavior Type` back to `Default`.
	- In the `Multiple Debris` script, we want to change the `Max Steps` to some higher number than before, `7000` will suit us. We want the agent to have more time to collect more junk.

### Code
- Our goal should now be a list of goals.
	- We should initialize this to be so many units long.
		- I am doing this with a `SerializableField` to set it, but may change this to count the number of children in a specific `Goals` parent object.
			- There are a couple of ways to do this with varying usefulness, but not important to the base idea, and not worth going into detail on the best method for such a small and relatively uncomplicated project.
- We need to make changes to the `OnEpisodeBegin` method as well to account for this.
	- In a `for` loop, iterate over the goals, doing the same as before (`setActive` and placement).
	- To add a little challenge, change the distance from a flat `5f` to a random range.
- We also need to make changes to the `OnTriggerEnter` method, to only end the episode if you've hit the number of goals.
- It's also a good time to have a modified `yml` file.
	- Change the name.
	- Increase the `Max Steps` to account for the increased complexity.

### Editor Changes
- Make your new arena prefab, then duplicate it a few times for training.

### Training and Beyond
- As before, run the training:
`mlagents-learn config/MultipleDebris.yml --run-id=MultipleDebrisTraining1`
- Then run the scene from within Unity
- When completed, as before, there will be a brain in your `results` folder, an `onnx` file.
- You can now apply this brain to your agent (remember to change from `Default` to `Inference Only`!)
