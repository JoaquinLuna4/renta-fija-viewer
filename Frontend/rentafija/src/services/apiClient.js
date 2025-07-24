import axios from "axios";
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL_PROD;

// Crea una instancia de Axios
const apiClient = axios.create({
	baseURL: API_BASE_URL,
	timeout: 1000000, // Establece un tiempo de espera de 1000000 milisegundos
	headers: {
		"Content-Type": "application/json",
	},
});

export default apiClient;
