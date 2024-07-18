# cf-bet
## Overview

VIDEO HERE

cf-bet is a sports betting application for Codeforces competitions. Users can select a type of bet, and the backend will perform a Monte-Carlo simulation of the competition. Probabilities and lines are generated from the simulation, determining the payout for a given bet.

## Features
- **Three Types of Bets:** Users can select two competitors to go head-to-head, a competitor to win the contest, or a competitor to achieve a specific placement.
- **Login System:** Users can register and log in to their accounts to track their previous bets and view statistics about their bets. They can also view these statistics on other users' profiles.
- **Monte-Carlo Simulation:** Simulates thousands of competition runs based on the competitors' ELO ratings, accounting for the randomness of competitors' performance.

## Technology Stack
**Technologies used:**
- **TypeScript:** For front-end development
- **ASP.NET:** For back-end development
- **React:** Front-end library
- **NextUI:** React UI Library
- **Axios:** Library for making API calls from the front-end

### Frontend

The frontend is a React application designed to provide an interactive user experience. It uses various libraries and tools to manage data retrieval, state management, and UI rendering. Data retrieval is handled by `axios`, which allows for HTTP requests to the backend. An example sequence of user inputs is described below:

1. The user accesses the site, which requests the current contests, bets, and rankings from the backend. This can be done without logging in.
2. The user can then log in with their Codeforces handle, storing a JWT token in their HTTP cookies.
3. When placing a bet, this JWT token is passed between the frontend and the server to authenticate the user.
4. The user can bet on any upcoming contest, choosing one of three types of bets.
5. The frontend updates when the contest ends, allowing the user to see if their bet was successful.

### Server

The server has two main functions: updating the database and calculating betting probabilities.

**Updating the database:**
The database is updated every 5 minutes with new data from the Codeforces API (to avoid exceeding rate limits). This includes getting new contests, updating the results of finished contests, and fetching the competitors of a specific contest. The data is stored in MongoDB Atlas using schemas.

**Calculating betting probabilities:**
The calculations for the probabilities of a given bet vary based on the type of bet:
1. For a bet pitting two competitors against each other, the probability that the competitor with rating ($R_a$) will beat a competitor with rating ($R_b$) is given by:
   
$$
P = \frac{1}{1 + 10^{\frac{R_b - R_a}{400}}}
$$
   
2. For a bet predicting a user's placement, the probability is calculated using a Monte-Carlo simulation. The rationale behind using this simulation is twofold: calculating the probability of a random system of 250 competitors is non-trivial, and the deviation between a competitor's performance level and skill level is considered to be i.i.d. [(Paper)](https://arxiv.org/abs/2101.00400). The Monte-Carlo simulation generates 10,000 simulations by adding a value from a normally distributed model to their skill level and tracking the rankings of competitors.

The moneyline or American odds are then calculated with:

$$
\begin{cases}
    -\frac{p \times 100}{1 - p}, & \text{if } p > 0.5 \\
    \frac{100}{p} - 100, & \text{if } p \leq 0.5
\end{cases}
$$

## Installation and Usage
1. Clone this repository.
2. Install the required dependencies:
   - In the root of the repository and `./backend`, run `npm install`.
3. In the root of the repository and `./backend`, rename the `.env.example` file to `.env`.
4. Update the values in the new `.env` file with your API keys and port selection.
5. Run `npm run start` or `npm run dev` in the root of the repository and `./backend`.

See `usage.md` for details on how to register for cf-bet.

## Limitations
1. The Monte-Carlo simulation does not perfectly reflect real-life competition. For example, the chances that tourist (currently number 1 on Codeforces) places first are higher than they should be. This is due to inflation [(See here)](https://codeforces.com/blog/entry/20762).
2. Users may be unable to bet on certain competitors if they register at the last minute and betting has closed. There is no way to circumvent this; users can only bet on registered competitors for the contest.
3. Unlike sports betting, users cannot place bets after the contest has started.

## License

cf-bet is licensed under the MIT License. See [LICENSE](./LICENSE) for more details.
