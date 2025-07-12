import * as React from "react";
import PropTypes from "prop-types";
import Box from "@mui/material/Box";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TablePagination from "@mui/material/TablePagination";
import TableRow from "@mui/material/TableRow";
import TableSortLabel from "@mui/material/TableSortLabel";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import Paper from "@mui/material/Paper";
import Checkbox from "@mui/material/Checkbox";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import FormControlLabel from "@mui/material/FormControlLabel";
import Switch from "@mui/material/Switch";

import { visuallyHidden } from "@mui/utils";

// --- Funciones auxiliares para ordenamiento de la tabla ---
function descendingComparator(a, b, orderBy) {
	if (b[orderBy] < a[orderBy]) {
		return -1;
	}
	if (b[orderBy] > a[orderBy]) {
		return 1;
	}
	return 0;
}

function getComparator(order, orderBy) {
	return order === "desc"
		? (a, b) => descendingComparator(a, b, orderBy)
		: (a, b) => -descendingComparator(a, b, orderBy);
}
// --- FIN Funciones auxiliares ---

// --- EnhancedTableHead - Encabezado de la tabla con ordenamiento ---
function EnhancedTableHead(props) {
	const { order, orderBy, onRequestSort, headCells } = props;
	const createSortHandler = (property) => (event) => {
		onRequestSort(event, property);
	};

	return (
		<TableHead
			sx={{
				position: "sticky",
				top: 0,
				zIndex: 1,
			}}
		>
			<TableRow>
				{headCells.map((headCell) => (
					<TableCell
						key={headCell.id}
						align={"center"}
						sortDirection={orderBy === headCell.id ? order : false}
						sx={{
							backgroundColor: "background.paper", // Usa el color de fondo del Paper del tema
							color: "text.primary",
							fontSize: "1.1rem",
							margin: 0,
							padding: 0,
						}}
					>
						<TableSortLabel
							active={orderBy === headCell.id}
							direction={orderBy === headCell.id ? order : "asc"}
							onClick={createSortHandler(headCell.id)}
						>
							{headCell.label}
							{orderBy === headCell.id ? (
								<Box component="span" sx={visuallyHidden}>
									{order === "desc" ? "sorted descending" : "sorted ascending"}
								</Box>
							) : null}
						</TableSortLabel>
					</TableCell>
				))}
			</TableRow>
		</TableHead>
	);
}

// PropTypes para EnhancedTableHead
EnhancedTableHead.propTypes = {
	onRequestSort: PropTypes.func.isRequired,
	order: PropTypes.oneOf(["asc", "desc"]).isRequired,
	orderBy: PropTypes.string.isRequired,
	rowCount: PropTypes.number.isRequired,
	headCells: PropTypes.array.isRequired,
};
// --- FIN EnhancedTableHead ---

// --- EnhancedTableToolbar - Barra de herramientas de la tabla ---
function EnhancedTableToolbar() {
	return (
		<Toolbar
			sx={{
				pl: { sm: 2 },
				pr: { xs: 1, sm: 1 },
			}}
		>
			{/* <Typography
				sx={{ flex: "1 1 100%" }}
				variant="h6"
				id="tableTitle"
				component="div"
			>
				Títulos de Renta Fija
			</Typography> */}
		</Toolbar>
	);
}

// PropTypes para EnhancedTableToolbar (simplificado ya que no hay selección)
EnhancedTableToolbar.propTypes = {
	// numSelected: PropTypes.number.isRequired, // No es necesario si no hay selección
};
// --- FIN EnhancedTableToolbar ---

// --- EnhancedTable - Componente principal de la tabla ---
// Recibe 'rows' (datos de la API) y 'headCells' (la definición de las columnas) como props
export default function EnhancedTable({ rows, headCells }) {
	const [order, setOrder] = React.useState("asc");
	// Establece el 'orderBy' inicial al 'id' de la primera columna si existe
	const [orderBy, setOrderBy] = React.useState(headCells[0]?.id || "");
	const [page, setPage] = React.useState(0);

	const [rowsPerPage, setRowsPerPage] = React.useState(5);

	const handleRequestSort = (event, property) => {
		const isAsc = orderBy === property && order === "asc";
		setOrder(isAsc ? "desc" : "asc");
		setOrderBy(property);
	};

	const handleChangePage = (event, newPage) => {
		setPage(newPage);
	};

	const handleChangeRowsPerPage = (event) => {
		setRowsPerPage(parseInt(event.target.value, 10));
		setPage(0); // Reinicia la página a 0 cuando cambian las filas por página
	};

	// Calcula las filas vacías para evitar saltos de diseño en la última página
	const emptyRows =
		page > 0 ? Math.max(0, (1 + page) * rowsPerPage - rows.length) : 0;

	// Filtra y ordena las filas visibles
	const visibleRows = React.useMemo(
		() =>
			[...rows] // Crea una copia para evitar mutar el array original
				.sort(getComparator(order, orderBy))
				.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
		[order, orderBy, page, rowsPerPage, rows] // Dependencias: recalcula si cambian
	);

	return (
		<Box sx={{ width: "100%" }}>
			<Paper sx={{ width: "100%", mb: 2, borderRadius: 2 }}>
				{/* <EnhancedTableToolbar /> */}
				<TableContainer sx={{ maxHeight: 440 }}>
					<Table sx={{ minWidth: 750 }} aria-labelledby="tableTitle">
						<EnhancedTableHead
							order={order}
							orderBy={orderBy}
							onRequestSort={handleRequestSort}
							rowCount={rows.length}
							headCells={headCells} // Pasa la definición de las columnas
						/>
						<TableBody>
							{visibleRows.map((row) => {
								// Cada fila necesita una 'key' única, usamos 'row.id' que ya procesamos
								return (
									<TableRow
										hover
										tabIndex={-1}
										key={row.id}
										sx={{ cursor: "pointer" }}
									>
										{/* Itera sobre headCells para renderizar las celdas de cada fila */}
										{headCells.map((headCell) => (
											<TableCell
												key={`${row.id}-${headCell.id}`} // Clave única para cada celda
												align={headCell.numeric ? "right" : "left"}
												// La primera columna (Bono) puede ser un <th> semánticamente
												component={
													headCell.id === headCells[0].id ? "th" : "td"
												}
												scope={
													headCell.id === headCells[0].id ? "row" : undefined
												}
												sx={{
													backgroundColor: "background.paper", // Usa el color de fondo del Paper del tema
													color: "text.primary",
													fontSize: "1rem",
													margin: 0,
													padding: "10px 20px",
												}}
											>
												{/* Accede al valor de la fila usando el 'id' de la headCell */}
												{row[headCell.id]}
											</TableCell>
										))}
									</TableRow>
								);
							})}
							{emptyRows > 0 && (
								<TableRow>
									<TableCell colSpan={headCells.length} />{" "}
								</TableRow>
							)}
						</TableBody>
					</Table>
				</TableContainer>
				<TablePagination
					rowsPerPageOptions={[5, 10, 25, 50, 100]}
					component="div"
					labelRowsPerPage={null}
					count={rows.length}
					rowsPerPage={rowsPerPage}
					page={page}
					onPageChange={handleChangePage}
					onRowsPerPageChange={handleChangeRowsPerPage}
					sx={{
						fontSize: "0.875rem",
					}}
				/>
			</Paper>
		</Box>
	);
}

// PropTypes para EnhancedTable
EnhancedTable.propTypes = {
	rows: PropTypes.array.isRequired,
	headCells: PropTypes.array.isRequired,
};
// --- FIN EnhancedTable ---
