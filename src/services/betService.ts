import axios from 'axios';

const BASE_URL = 'http://localhost:5228';

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
        console.error('Error placing compete bet:', error);
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
        console.error('Error placing winner bet:', error);
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
        console.error('Error placing top N bet:', error);
        throw error;
    }
};
