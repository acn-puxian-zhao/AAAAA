angular.module('app.dso.dsoanalysis', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

            .when('/dso/dsoanalysis', {
                templateUrl: 'app/dso/dsoanalysis.html',
                controller: 'dsoConfigCtrl',
                resolve: {
                }
            });
    }])

    .controller('dsoConfigCtrl',
    ['$scope', '$filter', 'modalService', '$interval', 'FileUploader', 'APPSETTING', 'myinvoicesProxy',
        function ($scope, $filter, modalService, $interval, FileUploader, APPSETTING, myinvoicesProxy
        ) {

        $scope.list = [];

        $scope.periodList = {
            multiSelect: false,
            enableFullRowSelection: false,
            noUnselect: true,
            data: 'list',
            columnDefs: [
                { field: 'sortId', displayName: '#', width: '60' },
                {
                    field: 'period', displayName: 'Period Date', width: '120',
                    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">{{row.entity.period | date:"yyyy-MM-dd"}}</div>'
                }
            ],
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            }
        };

        $scope.popup = {
            opened: false
        };

        $scope.open = function () {
            $scope.popup.opened = true;
        };

        $scope.addPeriod = function () {
            if ($scope.periodDate === null || $scope.periodDate === "" || $scope.periodDate === undefined) {
                alert("Please select new period's start date!");
                return;
            }
            else {
                $scope.list.push({ sortId: $scope.list.length + 1, period: $scope.periodDate });

            }
        };
        
        $scope.delPeriod = function () {
            var selected = $scope.gridApi.selection.getSelectedRows();
            if (selected && selected.length > 0) {
                if (confirm("are you sure to delete the period?") == true) {
                    var row = $scope.gridApi.selection.getSelectedRows()[0];
                    var index = $scope.list.indexOf(row);
                    $scope.list.splice(index, 1);
                }
            }
        };

        var uploader = $scope.uploader = new FileUploader({
            url: APPSETTING['serverUrl'] + '/api/Myinvoices/UploadDSO'
            });

        uploader.filters.push({
            name: 'customFilter',
            fn: function (item, options) {
                if (item.name.toString().toUpperCase().split(".")[1] != "XLS"
                    && item.name.toString().toUpperCase().split(".")[1] != "XLSX") {
                    alert("File format is not correct !");
                }
                return this.queue.length < 100;
            }
        });

        $scope.fileName = "";

        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            if (response != "") {
                $scope.fileName = response;
                alert("Upload Successed!");
            }
        };

        uploader.onErrorItem = function (fileItem, response, status, headers) {
            $scope.clearInfo();
        }

        $scope.updateLevel = function () {
            if (uploader.queue.length > 0 && uploader.queue[3] !== "" && $scope.getFileExtendName(uploader.queue[3]._file.name.toString()).toUpperCase() != ".ZIP") {
                alert("File format is not correct, \nPlease select the '.zip' file !");
                return;
            }
            if (uploader.queue[3] !== undefined && uploader.queue[3] != "") {
                uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices/UploadDSO';
            }
            else {
                alert("Please select file!");
                return;
            }
            uploader.uploadAll();
        };

        $scope.clearInfo = function () {
            if (uploader.queue.length > 0) {
                uploader.queue[3] = "";
                document.getElementById("updDSOFile").value = "";
                $scope.fileName = "";
            }
        }

        $scope.analysisPeriod = function () {
            if ($scope.fileName === undefined || $scope.fileName == "") {
                alert("Please select file and upload first!");
                return;
            }
            if ($scope.list.length <= 0) {
                alert("Please add period first!");
                return;
            }

            var monthList = "";
            angular.forEach($scope.list, function (each) {
                var strPeriod = $filter('date')(each.period, "yyyy-MM-dd");
                if (monthList == "") {
                    monthList = strPeriod;
                }
                else {
                    monthList += "," + strPeriod;
                }
            });

            if ($scope.packagedays === undefined || $scope.packagedays == null ) {
                alert("Please input package days!");
                return;
            }

            myinvoicesProxy.dsoAnalysis($scope.fileName, monthList, $scope.packagedays, function (path) {
                    window.location = path;
                    $scope.clearInfo();
                }
                , function () {
                    alert("Analysis Error");
                }
            );
        }

        $scope.getFileExtendName = function (str) {
            var d = /\.[^\.]+$/.exec(str);
            return d.toString();
        }

    }])