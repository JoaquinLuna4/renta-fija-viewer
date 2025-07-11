import React from "react";
import { useState, useEffect } from "react";

import { GridTable } from "../components/GridTable";
import { Container, Paper, Box } from "@mui/material";
import { fetchAssets } from "../services/assetsService";
import Loading from "../components/Loading";
const AllAssets = () => {
	const [rowsState, setRowsState] = useState([]);
	const [loading, setLoading] = useState(true);

	useEffect(() => {
		const getAssets = async () => {
			try {
				setLoading(true);
				const data = await fetchAssets();
				const processedData = data.map((item) => ({
					...item,
					id: item.Ticker, // Asigna el Ticker como el ID único
				}));
				setRowsState(processedData);
			} catch (error) {
				console.error("Error fetching assets:", error);
			} finally {
				setLoading(false);
			}
		};

		getAssets();
	}, []);

	const columns = [
		{ field: "nombreCompleto", headerName: "Bono", width: 150 },
		{ field: "id", headerName: "Ticker", width: 90 },
		{ field: "Vencimiento", headerName: "Vencimiento", width: 150 },
		{ field: "Cotización", headerName: "Cotización", width: 150 },
		{ field: "TIR Anual", headerName: "TIR Anual", width: 150 },
		{ field: "Paridad", headerName: "Paridad", width: 150 },
		{
			field: "Fecha Ultima Cotizacion",
			headerName: "Fecha Ultima Cotizacion",
			width: 150,
		},
	];
	// const rows = [
	// 	{ id: 1, name: "Asset 1", value: 100 },
	// 	{ id: 2, name: "Asset 2", value: 200 },
	// 	{ id: 3, name: "Asset 3", value: 300 },
	// ];

	return (
		<Container>
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
				<Paper elevation={3} sx={{ padding: 2, backgroundColor: "lightgray" }}>
					<GridTable
						rows={rowsState}
						columns={columns}
						pageSizeOptions={[5, 10, 20]}
						paginationModel={{ page: 0, pageSize: 10 }}
					/>
				</Paper>
			)}
		</Container>
	);
};

export default AllAssets;
