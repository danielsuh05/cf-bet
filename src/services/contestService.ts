import axios from "axios";

const BASE_URL = "http://localhost:5228";

export const getContests = async () => {
  try {
    const response = await axios.get(`${BASE_URL}/contests`);
    return response.data;
  } catch (error) {
    console.error("Error fetching contests:", error);
    throw error;
  }
};

export const getContestStatus = async (id: number) => {
  try {
    const response = await axios.get(`${BASE_URL}/conteststatus/${id}`);
    return response.data;
  } catch (error) {
    console.error("Error fetching contest status:", error);
    throw error;
  }
};

export const getRankings = async () => {
  try {
    const response = await axios.get(`${BASE_URL}/rankings`);
    return response.data;
  } catch (error) {
    console.error("Error fetching rankings:", error);
    throw error;
  }
};
