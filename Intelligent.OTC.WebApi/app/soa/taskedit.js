angular.module('app.taskedit', ['ui.bootstrap'])

    .controller('taskeditCtrl',
    ['$scope', '$filter', 'taskStatus', 'taskId', 'taskDate', 'taskType', 'taskContent', 'deal', 'legalEntity', 'customerNo', 'siteUseId', 'isAuto', '$uibModalInstance', 'baseDataProxy', 'taskProxy',
        function ($scope, $filter, taskStatus, taskId, taskDate, taskType, taskContent, deal, legalEntity, customerNo, siteUseId, isAuto, $uibModalInstance, baseDataProxy, taskProxy) {

            $scope.setTtitle = function () {
                if (taskId === undefined || taskId === "") {
                    $scope.title = "New Task";
                }
                else {
                    $scope.title = "Edit Task";
                }
            };
            $scope.setTtitle();

            if (isAuto === "1") {
                $scope.isauto = true;
            }
            else {
                $scope.isauto = false;
            }

            baseDataProxy.SysTypeDetail("042", function (tasktype) {
                $scope.taskTypeList = tasktype;
                $scope.taskType = taskType;
                if (!$scope.taskType || $scope.taskType === "") {
                    $scope.taskType = "催收记录";
                }
            });

            //$scope.taskStatusList = [
            //    { "detailValue": "0", "detailName": '待执行' },
            //    { "detailValue": "1", "detailName": '已完成' },
            //    { "detailValue": "2", "detailName": '已取消' }
            //];

            $scope.startDate = new Date(taskDate);
            $scope.taskContent = taskContent;
            $scope.taskStatus = taskStatus;
            if (!$scope.taskStatus || $scope.taskStatus === "") {
                $scope.taskStatus = "1";
            }

            $scope.popup = {
                opened: false
            };

            $scope.open = function () {
                $scope.popup.opened = true;
            };

            $scope.cancel = function () {
                result = "cancel";
                $uibModalInstance.close(result);
            };

            $scope.submit = function () {

                if (!$scope.startDate) {
                    alert("please input TaskDate.");
                    return;
                }
                if (!$scope.taskType) {
                    alert("please input TaskType.");
                    return;
                }
                if (!$scope.taskContent) {
                    alert("please input TaskContent.");
                    return;
                }

                var dateString = $filter('date')($scope.startDate, "yyyy-MM-dd"); 

                var result = []
                result.push('submit');
                if (taskId == '') {
                    //新建
                    taskProxy.newTask(deal, legalEntity, customerNo, siteUseId, dateString, $scope.taskType, $scope.taskContent, $scope.taskStatus, '0', function () {
                        $uibModalInstance.close(result);
                    })
                }
                else {
                    //修改
                    taskProxy.saveTask(taskId, dateString, $scope.taskType, $scope.taskContent, $scope.taskStatus, function () {
                        $uibModalInstance.close(result);
                    })
                }
            };

        }])
