# cf-bet
## Overview

VIDEO HERE

cf-bet is a "sports betting" application for Codeforces competitons. The user can select a type of bet and the backend will perform a Monte-Carlo simulation of the competition. Probabilities and lines are generated from the simulation, which determine the payout for a certain bet. 

## Features  
- <strong>Three Types of Bets:</strong> Users can select two competitors to go head-to-head, a competitor to win the contest, or a competitor to get a certain variable placement.
- <strong>Login System:</strong> Users can register and login to their account to keep track of their previous bets and statistics about their bets. They can also see these statistics about other users' profiles. 
- <strong>Monte-Carlo Simulation:</strong> Simulates thousands of competition runs based on the ELO of the competitors, accounting for the randomness of competitors' performance. 

## Technologies Stack
<strong>Technologies used: </strong>
- <strong>TypeScript:</strong> Front-end development
- <strong>ASP.NET:</strong> Back-end development
- <strong>React:</strong> Front-end library
- <strong>NextUI:</strong> React UI Library
- <strong>Axios:</strong> Library to call APIs from the front-end

### Front end:

The frontend is a React application designed to provide an interactive user experience. This application leverages various libraries and tools to manage data retrieval, state management, and UI rendering. Data retrieval is done through `axios`, which allows for HTTP request methods to be sent to the backend. An example sequence of user inputs is below:

The user first gets onto the site, where it will request the current contests, bets, and rankings from the backend. This can be done without logging in. The user can then log in with their Codeforces handle, where a JWT token is stored in their HTTP cookies. When trying to bet, this JWT token is passed between the front-end and the server to authenticate the user. The user can then bet on any upcoming contest, choosing one of three different types of bets. The front-end gets updated when the contest ends, in which case the user can see if their bet hit or not. 

### Server:

The server has two main purposes: update the database and calculate betting probabilities. 

<strong>Updating the database:</strong> The database is updated every 5 minutes with new data from the Codeforces API (as to not exceed rate limits). This data includes getting new contests, updating the results of a finished contest, and getting the competitors of a specific contest. The data is stored in MongoDB Atlas using schemas.

<strong>Calculating betting probabilities:</strong> The calculations for the probabilities of a given bet differ based on what type of bet:
1. For a bet pitting two competitors againts each other, the probability that the competitor with rating $R_a$ will beat another competitor with rating $R_b$ is given by

$$P=\frac{1}{1+10^\frac{R_b-R_a}{400}}$$

2. For a bet predicting the user will have a certain placement, the probability is calculated using a Monte-Carlo simulation. The rationale behind using this simulation is twofold: calculating the probability of a random system of 250 competitors is non-trivial and the deviation between a competitor's performance level and skill level is considered to be i.i.d. [(Paper)](https://arxiv.org/abs/2101.00400). The Monte-Carlo simulation generates 10,000 simulations by adding a value from a normally distributed model to their skill level and keeping track of the rankings of competitors.

The moneyline or American odds is then calculated with 

$$
\begin{cases}
    -\frac{p\times100}{1-p},& \text{if } p\leq 0.5\\
    \frac{100}{p}-100,              & p>0.5
\end{cases}
$$

## Installation and Usage
1. Clone this repository
2. Install the required dependencies
    - In the root of the repository and `./backend`, run `npm i`
3. In the root of the repository and `./backend`, rename the `.env.example` file to `.env`
4. Change the values inside of the new `.env` file with your API keys and port selection
5. Run `npm run start` or `npm run dev` in the root of the repository and `./backend`

See `usage.md` for details on how to register for cf-bet. 

## License

cf-bet is under the MIT License. See [LICENSE](./LICENSE) for more details.
