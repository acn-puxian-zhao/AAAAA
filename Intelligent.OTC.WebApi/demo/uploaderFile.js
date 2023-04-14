angular.module('ui.bootstrap.demo')

    .controller('uploaderController', ['$scope', 'FileUploader', function ($scope, FileUploader) {
        var uploader = $scope.uploader = new FileUploader({
            url: 'http://localhost:55209/api/UpdateFile'
        });

        // FILTERS

        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });

        // CALLBACKS

//        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
//            console.info('onWhenAddingFileFailed', item, filter, options);
//        };
//        uploader.onAfterAddingFile = function (fileItem) {
//            console.info('onAfterAddingFile', fileItem);
//        };
//        uploader.onAfterAddingAll = function (addedFileItems) {
//            console.info('onAfterAddingAll', addedFileItems);
//        };
//        uploader.onBeforeUploadItem = function (item) {
//            console.info('onBeforeUploadItem', item);
//        };
//        uploader.onProgressItem = function (fileItem, progress) {
//            console.info('onProgressItem', fileItem, progress);
//        };
//        uploader.onProgressAll = function (progress) {
//            console.info('onProgressAll', progress);
//        };
//        uploader.onSuccessItem = function (fileItem, response, status, headers) {
//            console.info('onSuccessItem', fileItem, response, status, headers);
//        };
//        uploader.onErrorItem = function (fileItem, response, status, headers) {
//            console.info('onErrorItem', fileItem, response, status, headers);
//        };
//        uploader.onCancelItem = function (fileItem, response, status, headers) {
//            console.info('onCancelItem', fileItem, response, status, headers);
//        };
//        uploader.onCompleteItem = function (fileItem, response, status, headers) {
//            console.info('onCompleteItem', fileItem, response, status, headers);
//        };
//        uploader.onCompleteAll = function () {
//            console.info('onCompleteAll');
//        };

        //console.info('uploader', uploader);

        $scope.levelList = [
            {
                name: 'LEVEL', id: 'level', levels: [
                    { "id": 1, "levelName": '--Select Level--' },
                    { "id": 2, "levelName": 'Account Level' },
                    { "id": 3, "levelName": 'Invoice Level' }]
            }
        ];

        $scope.selectedLevels = [];

        $scope.$watchCollection('selectedLevels', function () {
            $scope.selectedLevel = angular.copy($scope.selectedLevels[0]);
        });

//        $scope.clearCityAndZip = function () {
//            $scope.selectedUsers[0].city = null;
//            $scope.selectedUsers[0].zip = "";
//        };

        $scope.$watch('selectedLevel[0].levels', function (selectedLevelId) {
            if (selectedLevelId) {
                angular.forEach($scope.selectedLevel[0].levels, function (level) {
                    if (selectedLevelId == level.id) {
                        $scope.selectedLevel = level;
                    }
                });
            }
        });
        $scope.openDialog = function () {
            // Inlined template for demo

            var modalDefaults = {
                templateUrl: 'demo/uploaderFile.html',
                controller: function ($scope) {
                    $scope.closeButtonText= 'Cancel',
                    $scope.actionButtonText= 'Commit',
                    $scope.headerText= 'Edit Dialog'
                    $scope.ok = function () {
                        alert('ok clicked');
                    };                
                    $scope.close = function () {
                        alert('close clicked');
                    };
    //                $scope.selectedUser = $scope.$parent.selectedUser;

    //                $scope.selectedUser = [];
                    $scope.selectedLevel.firstName = 'name';
                }
            };

            var modalOptions = {
                selectedLevel: $scope.selectedLevel,
            };

            modalService.showModal(modalDefaults, modalOptions).then(function (result) {
                // something you want to do after commit.

            });
        }

    } ]);
