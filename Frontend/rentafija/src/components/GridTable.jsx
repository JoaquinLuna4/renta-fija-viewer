import Paper from "@mui/material/Paper";
import { DataGrid } from "@mui/x-data-grid/DataGrid";
export const GridTable = ({
	rows,
	columns,
	paginationModel,
	pageSizeOptions,
	sx,
}) => {
	return (
		<Paper sx={{}}>
			<DataGrid
				rows={rows}
				columns={columns}
				initialState={{ pagination: { paginationModel } }}
				pageSizeOptions={pageSizeOptions}
				sx={{
					border: 8,

					...sx,
				}}
			/>
		</Paper>
	);
};
