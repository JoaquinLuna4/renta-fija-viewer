import React from "react";
import { useState, useEffect, useMemo } from "react";
import EnhancedTable from "../components/TableCustom";
import { GridTable } from "../components/GridTable";
import { Container, Paper, Box, TextField, Typography } from "@mui/material";
import { fetchAssets } from "../services/assetsService";
import Loading from "../components/Loading";
import formatDate from "../utils/formatDate";

// Definición de headCells para tus datos de bonos
// El 'id' de cada headCell DEBE coincidir con la clave de la propiedad en los objetos de datos (rows)
const headCells = [
	{
		id: "nombreCompleto",
		numeric: false,

		label: "Bono",
	},
	{ id: "tipoActivo", numeric: false, label: "Tipo activo" },
	{ id: "Ticker", numeric: false, label: "Ticker" },
	{
		id: "Vencimiento",
		numeric: false,

		label: "Vencimiento",
	},
	{
		id: "Cotización",
		numeric: true,

		label: "Cotización",
	},
	{
		id: "TIR Anual",
		numeric: true,

		label: "TIR Anual (%)",
	},
	{ id: "Paridad", numeric: true, label: "Paridad" },
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
	// 	{
	// 		field: "Fecha Ultima Cotizacion",
	// 		headerName: "Fecha Ultima Cotizacion",
	// 		width: 150,
	// 	},
	// ]
	// const rows = [
	// 	{ id: 1, name: "Asset 1", value: 100 },
	// 	{ id: 2, name: "Asset 2", value: 200 },
	// 	{ id: 3, name: "Asset 3", value: 300 },
	// ];

	return (
		<Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
			<Typography
				variant="h4"
				color="text.secondary"
				component="h1"
				gutterBottom
				align="center"
			>
				Activos de Renta Fija
			</Typography>
			<Paper elevation={3} sx={{ padding: 2, borderRadius: 2 }}>
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
							variant="body1"
							component="span"
							sx={{ fontWeight: "bold" }}
						>
							{dateReport}
						</Typography>
					</Typography>
				</Box>
				<TextField
					fullWidth
					label="Bono , Ticker, Vencimiento..."
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
					<EnhancedTable
						rows={filteredRows}
						headCells={headCells}
						// ------- Definicion de props para DataGrid de Mui -----  //
						// rows={rowsState}
						// columns={columns}
						// pageSizeOptions={[5, 10, 20]}
						// paginationModel={{ page: 0, pageSize: 10 }}
					/>
				)}
			</Paper>
		</Container>
	);
};

export default AllAssets;
