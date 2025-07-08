import axios from "axios";
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Crea una instancia de Axios
const apiClient = axios.create({
	baseURL: API_BASE_URL,
	timeout: 5000, // Establece un tiempo de espera de 5 segundos
	headers: {
		"Content-Type": "application/json",
	},
});

export default apiClient;
