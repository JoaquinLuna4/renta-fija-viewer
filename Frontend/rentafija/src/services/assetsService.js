import apiClient from "./apiClient";

export const fetchAssets = async () => {
	try {
		const response = await apiClient.get("/RentaFija");

		return response.data;
	} catch (error) {
		console.error("Error fetching assets:", error);
		throw error;
	}
};
