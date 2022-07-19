export interface PipelineCandidate {
	pipelineId: string;
	startedAt: Date;
	completedAt: Date;
	batchNumber: number;
	taskId: number;
	datasetId: number;
	taskTypeId: string;
	createdBy: string;
	aborted?: boolean;
	sourceFileName: string;
	rewardFunctionType: string;
	simulationDuration: number;
	actionsCount: number;
}
