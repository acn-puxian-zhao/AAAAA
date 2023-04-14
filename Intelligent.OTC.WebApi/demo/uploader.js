angular.module('ui.bootstrap.demo')

    .controller('ModalUploaderCtrl', function ($scope, $modal, $log) {

        $scope.open = function (size) {
            var modalUpload = $modal.open({
                animation: true,
                templateUrl: 'myUplFileContent.html',
                controller: 'uploaderController',
                size: size

            });
            
        };
    });

angular.module('ui.bootstrap.demo')

    .controller('uploaderController', ['$scope', 'FileUploader', 'APPSETTING', function ($scope, FileUploader, APPSETTING) {

        var uploader = $scope.uploader = new FileUploader({
            url: APPSETTING['serverUrl'] + '/api/UpdateFile' + '?levelFlag=1'
        });

        // FILTERS

        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });

        $scope.levelList = [
            {
                name: 'LEVEL', id: 'level', levels: [
                    { "id": 1, "levelName": 'Account Level' },
                    { "id": 2, "levelName": 'Invoice Level' }]
            }
        ];

        $scope.selectedLevels = [];

        $scope.$watchCollection('selectedLevels', function () {
            $scope.selectedLevel = angular.copy($scope.selectedLevels[0]);
        });

//        $scope.updateLevel = function (levels) {
//            levelFlag = levels.toString;
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
