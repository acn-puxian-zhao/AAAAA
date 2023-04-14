angular.module('app.common.importContactor', [])

    .controller('importContactorCtrl', ['$scope', 'FileUploader','APPSETTING','$uibModalInstance',

        function ($scope, FileUploader,APPSETTING,$uibModalInstance) {
            var uploader = $scope.uploader = new FileUploader();

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item, options) {
                    return this.queue.length < 10;
                }
            });

            // CALLBACKS
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                document.getElementById("overlay-container").style.display = "none";
                document.getElementById("overlay-container").style.flg = "";
                uploader.queue[3] = "";
                document.getElementById("importFile").value = "";
                alert(response);
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                document.getElementById("overlay-container").style.display = "none";
                document.getElementById("overlay-container").style.flg = "";
                uploader.queue[3] = "";
                alert(response);
            };

            $scope.importExcel = function () {
                if (uploader.queue[3] == null || uploader.queue[3]=="") {
                    alert("Please select File");
                    return;
                }
                var type = 'importcontactor';
                document.getElementById("overlay-container").style.display = "block";
                document.getElementById("overlay-container").style.flg = "loading";
                uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Customer?Type=' + type;
                uploader.uploadAll();
            }
            $scope.close = function () {
                $uibModalInstance.close();
            };
            $scope.reset = function () {
                uploader.queue[3] = "";
                document.getElementById("importFile").value = "";
            };
        }   

]);
