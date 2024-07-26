import axios from "axios";

const BASE_URL = "http://localhost:3000";

const getAuthHeader = (token: string) => ({
  headers: {
    Authorization: `Bearer ${token}`,
  },
});

export const placeCompeteBet = async (token: string, betData: any) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/bet/compete`,
      betData,
      getAuthHeader(token)
    );
    return response.data;
  } catch (error) {
    console.error("Error placing compete bet:", error);
    throw error;
  }
};

export const placeWinnerBet = async (token: string, betData: any) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/bet/winner`,
      betData,
      getAuthHeader(token)
    );
    return response.data;
  } catch (error) {
    console.error("Error placing winner bet:", error);
    throw error;
  }
};

export const placeTopNBet = async (token: string, betData: any) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/bet/topn`,
      betData,
      getAuthHeader(token)
    );
    return response.data;
  } catch (error) {
    console.error("Error placing top N bet:", error);
    throw error;
  }
};

export const getCompeteBetDetails = async (token: string, betData: any) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/bet/compete/details`,
      betData,
      getAuthHeader(token)
    );
    return response.data;
  } catch (error) {
    console.error("Error fetching compete bet details:", error);
    throw error;
  }
};

export const getWinnerBetDetails = async (token: string, betData: any) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/bet/winner/details`,
      betData,
      getAuthHeader(token)
    );
    return response.data;
  } catch (error) {
    console.error("Error fetching winner bet details:", error);
    throw error;
  }
};

export const getTopNBetDetails = async (token: string, betData: any) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/bet/topn/details`,
      betData,
      getAuthHeader(token)
    );
    return response.data;
  } catch (error) {
    console.error("Error fetching top N bet details:", error);
    throw error;
  }
};
