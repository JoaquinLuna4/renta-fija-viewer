import React from "react";
import { useState, useEffect, useMemo } from "react";
import EnhancedTable from "../components/TableCustom";
import { Container, Paper, Box, Typography, TextField } from "@mui/material";
import Navbar from "../components/Navbar";
import { fetchAssets } from "../services/assetsService";
import Loading from "../components/Loading";
import formatDate from "../utils/formatDate";

// Definición de headCells para los datos de bonos
// El 'id' de cada headCell DEBE coincidir con la clave de la propiedad en los objetos de datos (rows)
const headCells = [
	{
		id: "Especie",
		numeric: false,
		label: "Especie",
		width: 150,
	},
	{
		id: "Tasa de licitación",
		numeric: true,
		label: "Tasa de Licitación",
		width: 120,
		format: (value) => `${Number(value).toFixed(2)}%`,
	},
	{
		id: "Precio ARS c/VN 100",
		numeric: true,
		label: "Precio",
		width: 100,
		format: (value) => `$${Number(value).toFixed(2)}`,
	},
	{
		id: "Rendimiento del Período",
		numeric: true,
		label: "Rend. Período",
		width: 120,
		format: (value) => `${Number(value).toFixed(2)}%`,
	},
	{
		id: "TNA",
		numeric: true,
		label: "TNA",
		width: 100,
		format: (value) => `${Number(value).toFixed(2)}%`,
	},
	{
		id: "TEA",
		numeric: true,
		label: "TEA",
		width: 100,
		format: (value) => `${Number(value).toFixed(2)}%`,
	},
	{
		id: "TEM",
		numeric: true,
		label: "TEM",
		width: 100,
		format: (value) => `${Number(value).toFixed(2)}%`,
	},
	{
		id: "DM",
		numeric: true,
		label: "Duración Mod.",
		width: 120,
		format: (value) => Number(value).toFixed(2),
	},
	/*{
		id: "Plazo al Vto(Días)",
		numeric: true,
		label: "Días al Vto",
		width: 100,
	},*/
	{
		id: "Monto al Vto",
		numeric: true,
		label: "Monto Vto.",
		width: 120,
		format: (value) => `$${Number(value).toLocaleString("es-AR")}`,
	},
	{
		id: "Fecha de Emisión",
		numeric: false,
		label: "Emisión",
		width: 100,
	},
	{
		id: "Fecha de Pago",
		numeric: false,
		label: "Pago",
		width: 100,
	},
	{
		id: "FechaCierre",
		numeric: false,
		label: "Cierre",
		width: 100,
	},
	{
		id: "FechaLiquidacion",
		numeric: false,
		label: "Liquidación",
		width: 100,
	},
];

const AllAssets = () => {
	const [rowsState, setRowsState] = useState([]);
	const [loading, setLoading] = useState(true);
	const [dateReport, setDateReport] = useState("");

	const [searchTerm, setSearchTerm] = useState(""); //Estado para el filtro

	useEffect(() => {
		const getAssets = async () => {
			try {
				setLoading(true);
				const data = await fetchAssets();
				console.log(data);

				const processedData = data.activos.map((item) => ({
					...item,
					id: item.Ticker, // Asigna el Ticker como el ID único
				}));
				setRowsState(processedData);
				setDateReport(formatDate(data.fechaInforme));
			} catch (error) {
				console.error("Error fetching assets:", error);
			} finally {
				setLoading(false);
			}
		};

		getAssets();
	}, []);

	// --- Función para manejar el cambio en la SearchBar ---
	const handleSearchChange = (event) => {
		setSearchTerm(event.target.value);
	};
	// ---  Lógica de filtrado de los datos ---
	const filteredRows = useMemo(() => {
		if (!searchTerm) {
			return rowsState; // Si no hay término de búsqueda, devuelve todas las filas
		}
		const lowerCaseSearchTerm = searchTerm.toLowerCase();
		return rowsState.filter((row) => {
			// Itera sobre los valores de cada columna para ver si alguno coincide
			// con el término de búsqueda. Puedes ajustar qué columnas buscar.
			return headCells.some((headCell) => {
				const value = row[headCell.id];
				// Convertir a string para buscar, manejar nulos/undefined
				if (value === null || value === undefined) return false;
				return String(value).toLowerCase().includes(lowerCaseSearchTerm);
			});
		});
	}, [rowsState, searchTerm]); // Dependencias: recalcula si cambian las filas o el término

	// ------------ Definición de columnas para la tabla usando el componente DataGrid de MUI ------------
	// const columns = [
	// 	{ field: "nombreCompleto", headerName: "Bono", width: 150 },
	// 	{ field: "id", headerName: "Ticker", width: 90 },
	// 	{ field: "Vencimiento", headerName: "Vencimiento", width: 150 },
	// 	{ field: "Cotización", headerName: "Cotización", width: 150 },
	// 	{ field: "TIR Anual", headerName: "TIR Anual", width: 150 },
	// 	{ field: "Paridad", headerName: "Paridad", width: 150 },
	return (
		<>
			<Container maxWidth={false} sx={{ mt: 4, mb: 4, width: "95%" }}>
				<Typography
					variant="h3"
					component="h1"
					gutterBottom
					align="center"
					sx={{
						background: (theme) => theme.palette.primary.contrastText,
						"& *": {
							color: "inherit",
						},
						WebkitBackgroundClip: "text",
						WebkitTextFillColor: "transparent",
						fontWeight: 700,
						letterSpacing: ".2rem",
						mb: 4,
					}}
				>
					Activos de Renta Fija
				</Typography>
				<Paper
					elevation={3}
					sx={{
						padding: 3,
						borderRadius: 2,
						width: "100%",
						maxWidth: "100%",
						overflowX: "auto",
					}}
				>
					<Box>
						<Typography
							variant="h5"
							component="h2"
							gutterBottom
							align="right"
							sx={{ mb: 2, color: "text.primary" }}
						>
							Fecha Informe:{" "}
							<Typography
								variant="h6"
								component="span"
								sx={{ fontWeight: "bold", fontStyle: "italic" }}
							>
								{dateReport}
							</Typography>
						</Typography>
					</Box>
					<TextField
						fullWidth
						label="Especie , TNA, Vencimiento..."
						variant="outlined"
						value={searchTerm}
						onChange={handleSearchChange}
						sx={{ mb: 3 }}
					/>

					{loading ? (
						<Box
							sx={{
								display: "flex",
								justifyContent: "center",
								alignItems: "center",
								height: 300,
								width: "100%",
							}}
						>
							<Loading />
						</Box>
					) : (
						<EnhancedTable rows={filteredRows} headCells={headCells} />
					)}
				</Paper>
			</Container>
		</>
	);
};
export default AllAssets;
