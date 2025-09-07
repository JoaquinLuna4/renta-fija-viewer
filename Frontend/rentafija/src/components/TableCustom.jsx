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
import Paper from "@mui/material/Paper";
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
				backgroundColor: "background.paper",
			}}
		>
			<TableRow>
				{headCells.map((headCell) => (
					<TableCell
						key={headCell.id}
						align={headCell.numeric ? "right" : "left"}
						sortDirection={orderBy === headCell.id ? order : false}
						sx={{
							backgroundColor: "background.paper",
							color: "text.primary",
							fontWeight: "bold",
							fontSize: "0.85rem",
							padding: "8px 12px",
							whiteSpace: "nowrap",
							width: headCell.width ? `${headCell.width}px` : "auto",
							minWidth: headCell.width ? `${headCell.width}px` : "auto",
							borderBottom: "2px solid",
							borderColor: "divider",
						}}
					>
						<TableSortLabel
							active={orderBy === headCell.id}
							direction={orderBy === headCell.id ? order : "asc"}
							onClick={createSortHandler(headCell.id)}
							sx={{
								"&:hover": {
									color: "primary.main",
								},
							}}
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
	const [orderBy, setOrderBy] = React.useState(headCells[0]?.id || "");
	const [page, setPage] = React.useState(0);
	const [rowsPerPage, setRowsPerPage] = React.useState(10);

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
		setPage(0);
	};

	// Función para formatear el valor de la celda según la definición de la columna
	const formatCellValue = (value, headCell) => {
		if (value === null || value === undefined) return "";
		if (headCell.format && typeof headCell.format === "function") {
			try {
				return headCell.format(value);
			} catch (error) {
				console.error(
					`Error formateando valor para ${headCell.id}:`,
					value,
					error
				);
				return value;
			}
		}
		return value;
	};

	// Calcula las filas vacías para evitar saltos de diseño en la última página
	const emptyRows =
		page > 0 ? Math.max(0, (1 + page) * rowsPerPage - rows.length) : 0;

	// Filtra y ordena las filas visibles
	const visibleRows = React.useMemo(
		() =>
			[...rows]
				.sort(getComparator(order, orderBy))
				.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
		[order, orderBy, page, rowsPerPage, rows]
	);

	// Calcula el ancho total de la tabla
	const tableWidth = headCells.reduce(
		(acc, cell) => acc + (cell.width || 120),
		0
	);

	return (
		<Box sx={{ width: "100%", overflow: "auto" }}>
			<Paper sx={{ width: "100%", mb: 2, borderRadius: 2, overflow: "hidden" }}>
				<TableContainer
					sx={{
						maxHeight: "calc(100vh - 200px)",
						width: "100%",
						margin: "0 auto",
						padding: "0 16px",
					}}
				>
					<Table
						stickyHeader
						aria-labelledby="tableTitle"
						sx={{
							minWidth: Math.max(tableWidth, "100%"),
							tableLayout: "auto",
						}}
					>
						<EnhancedTableHead
							order={order}
							orderBy={orderBy}
							onRequestSort={handleRequestSort}
							rowCount={rows.length}
							headCells={headCells}
						/>
						<TableBody>
							{visibleRows.map((row) => (
								<TableRow
									hover
									tabIndex={-1}
									key={row.id}
									sx={{
										"&:nth-of-type(odd)": {
											backgroundColor: "action.hover",
										},
										"&:hover": {
											backgroundColor: "action.selected",
										},
									}}
								>
									{headCells.map((headCell) => {
										const cellValue = row[headCell.id];
										const formattedValue = formatCellValue(cellValue, headCell);

										return (
											<TableCell
												key={`${row.id}-${headCell.id}`}
												align={headCell.numeric ? "right" : "left"}
												component={
													headCell.id === headCells[0].id ? "th" : "td"
												}
												scope={
													headCell.id === headCells[0].id ? "row" : undefined
												}
												sx={{
													fontSize: "0.85rem",
													padding: "8px 12px",
													whiteSpace: "nowrap",
													width: headCell.width
														? `${headCell.width}px`
														: "auto",
													minWidth: headCell.width
														? `${headCell.width}px`
														: "auto",
													borderBottom: "1px solid rgba(224, 224, 224, 0.5)",
												}}
											>
												{formattedValue}
											</TableCell>
										);
									})}
								</TableRow>
							))}
							{emptyRows > 0 && (
								<TableRow style={{ height: 53 * emptyRows }}>
									<TableCell colSpan={headCells.length} />
								</TableRow>
							)}
						</TableBody>
					</Table>
				</TableContainer>
				<TablePagination
					rowsPerPageOptions={[10, 25, 50, 100]}
					component="div"
					count={rows.length}
					rowsPerPage={rowsPerPage}
					page={page}
					onPageChange={handleChangePage}
					onRowsPerPageChange={handleChangeRowsPerPage}
					sx={{
						"& .MuiTablePagination-toolbar": {
							padding: "8px 16px",
						},
					}}
					labelRowsPerPage="Filas por página:"
					labelDisplayedRows={({ from, to, count }) =>
						`${from}-${to} de ${count !== -1 ? count : `más de ${to}`}`
					}
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
