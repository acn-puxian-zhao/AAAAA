angular.module('app.common.importBadDebt', [])

    .controller('importBadDebtCtrl', ['$scope', 'FileUploader','APPSETTING','$uibModalInstance',

        function ($scope, FileUploader,APPSETTING,$uibModalInstance) {
            var uploader = $scope.uploader = new FileUploader({
                url: APPSETTING['serverUrl'] + '/api/Customer'
            });

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    return this.queue.length < 10;
                }
            });

            // CALLBACKS
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                document.getElementById("overlay-container").style.display = "none";
                //added by zhangYu  start resove upload file loading dia is missing
                document.getElementById("overlay-container").style.flg = "";
                //aviod click [upload] again,to clear
                uploader.queue[3] = "";
                document.getElementById("importFile").value = "";
                //added by zhangYu  start resove upload file loading dia is missing
                alert(response);
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                document.getElementById("overlay-container").style.display = "none";
                //added by zhangYu  start resove upload file loading dia is missing
                document.getElementById("overlay-container").style.flg = "";
                //added by zhangYu  start resove upload file loading dia is missing
                alert(response);
            };

            $scope.importExcel = function () {
                if (uploader.queue[3] === null || uploader.queue[3]==="") {
                    alert("Please select File");
                    return;
                }
                var type = 'importBadDebt';
                document.getElementById("overlay-container").style.display = "block";
                //added by zhangYu   resove upload file loading dia is missing  start
                document.getElementById("overlay-container").style.flg = "loading";
                //added by zhangYu  resove upload file loading dia is missing end
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
