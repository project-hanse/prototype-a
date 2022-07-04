export const environment = {
	production: true,
	pipelineApi: 'https://hanse.allteams.at/api/pipeline',
	datasetApi: 'https://hanse.allteams.at/api/dataset',
	filesApi: 'https://hanse.allteams.at/api/file', // TODO: change to /api/file/ and update edge server config
	learningApi: 'https://hanse.allteams.at/api/learning',
	mlflow: 'https://hanse.allteams.at/mlflow/#',
	adminer: 'https://hanse.allteams.at/adminer/#',
	messageBrokerHost: 'hanse.allteams.at',
	messageBrokerPath: '/mqtt',
	messageBrokerPort: 443
};
