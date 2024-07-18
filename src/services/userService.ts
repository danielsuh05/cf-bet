import axios from "axios";

const BASE_URL = "http://localhost:5228";

export const register = async (username: string, password: string) => {
  try {
    const response = await axios.post(`${BASE_URL}/register`, {
      username,
      password,
    });
    return response.data;
  } catch (error) {
    console.error("Error registering user:", error);
    throw error;
  }
};

export const login = async (username: string, password: string) => {
  try {
    const response = await axios.post(`${BASE_URL}/login`, {
      username,
      password,
    });
    return response.data;
  } catch (error) {
    console.error("Error logging in user:", error);
    throw error;
  }
};

export const getUserInfo = async (username: string) => {
  try {
    const response = await axios.get(`${BASE_URL}/user/${username}`);
    return response.data;
  } catch (error) {
    console.error("Error fetching user bets:", error);
    throw error;
  }
};

export const getUserBets = async (username: string) => {
  try {
    const response = await axios.get(`${BASE_URL}/userbets/${username}`);
    return response.data;
  } catch (error) {
    console.error("Error fetching user bets:", error);
    throw error;
  }
};

export const getUserContestBets = async (
  username: string,
  contestId: number,
  token: string
) => {
  try {
    const response = await axios.get(
      `${BASE_URL}/usercontestbets/${username}:${contestId}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error fetching user contest bets:", error);
    throw error;
  }
};
