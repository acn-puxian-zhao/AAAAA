angular.module('app.reuploadPostClear', ['ui.bootstrap'])

    .controller('reuploadPostClearCtrl', 
    // 'siteuseid','legalentity',
        ['$scope', '$filter', '$interval', 'caHisDataProxy', 'caCommonProxy', 'modalService', '$location', 'FileUploader', 'APPSETTING', '$uibModalInstance',
            function ($scope, $filter, $interval, caHisDataProxy, caCommonProxy, modalService, $location, FileUploader, APPSETTING, $uibModalInstance) {

                var uploader = $scope.uploader = new FileUploader({
                    url: APPSETTING['serverUrl'] + '/api/caBankStatementController/reuploadPostClear'
                });

                uploader.filters.push({
                    name: 'uploadFilter',
                    fn: function (item /*{File|FileLikeObject}*/, options) {
                        if (//item.name.toString().toUpperCase().split(".")[1] != "TXT"
                            item.name.toString().toUpperCase().split(".")[1] !== "XLS"
                            //&& item.name.toString().toUpperCase().split(".")[1] != "ZIP"
                            && item.name.toString().toUpperCase().split(".")[1] !== "XLSX") {
                            alert("File format is not correct (.xls & .xlsx) !");
                        }
                        return this.queue.length < 100;
                    }
                });


                uploader.onSuccessItem = function (fileItem, response, status, headers) {
                    document.getElementById("overlay-container").style.display = "none";
                    document.getElementById("overlay-container").style.flg = "";

                    result = "submit";
                    $uibModalInstance.close(result);

                    alert(response)
                };

                uploader.onErrorItem = function (fileItem, response, status, headers) {
                    document.getElementById("overlay-container").style.display = "none";
                    document.getElementById("overlay-container").style.flg = "";

                    uploader.queue[9] = "";
                    $scope.clearFile("updBSFile");
                    uploadType = "";
                    alert(response);
                };


                $scope.updateBSFile = function () {
                    if (uploader.queue[9] === null || uploader.queue[9] === "" || uploader.queue[9] === undefined) {
                        alert("Please select the file you need to upload!");
                        return;
                    }
                    if (uploader.queue[9]._file.name.toString().toUpperCase().substring(uploader.queue[9]._file.name.toString().lastIndexOf(".") + 1) !== "XLS"
                        && uploader.queue[9]._file.name.toString().toUpperCase().substring(uploader.queue[9]._file.name.toString().lastIndexOf(".") + 1) !== "XLSX") {
                        alert("File format is not correct, \nPlease select the '.xls' or '.xlsx' file !");
                        return;
                    }

                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";

                    uploader.queue[9].url = APPSETTING['serverUrl'] + '/api/caBankStatementController/reuploadPostClear';
                    uploader.uploadAll();
                };

                $scope.clearFile = function (items) {
                    if (uploader.queue.length > 0) {
                        uploader.queue[0] = "";
                    }
                    document.getElementById(items).value = "";
                };

                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };
        }])
