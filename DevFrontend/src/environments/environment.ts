// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
	production: false,
	pipelineApi: 'http://localhost:5000',
	filesApi: 'http://localhost:5004',
	datasetApi: 'http://localhost:5002',
	learningApi: 'http://localhost:5006',
	mlflow: 'http://localhost:5005',
	adminer: 'http://localhost:8081',
	messageBrokerHost: 'localhost',
	messageBrokerPath: '',
	messageBrokerPort: 9002
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
