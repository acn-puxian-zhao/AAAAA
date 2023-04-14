angular.module('resources.taskProxy', []);
angular.module('resources.taskProxy').factory('taskProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('task');

    //Aging
    factory.queryTask = function (index, itemCount, filter, legalEntity, custNum, custName, siteUseId, startDate, status, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/query?pageindex=' + index + '&pagesize=' + itemCount + '&filter=' + filter
                + '&legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + '&startDate=' + startDate + '&status=' + status,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //PMT
    factory.queryPMTTask = function (legalEntity, custNum, custName, siteUseId, status, dateF, dateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/pmtquery?legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + '&status=' + status + "&dateF=" + dateF + "&dateT=" + dateT,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.exportPMT = function (legalEntity, custNum, custName, siteUseId, status, dateF, dateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/exportpmt?legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + '&status=' + status + "&dateF=" + dateF + "&dateT=" + dateT,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
	};

    factory.queryPMTDetailTask = function (siteUseId, balanceAmt, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/pmtdetailquery?siteUseId=' + siteUseId + '&balanceAmt=' + balanceAmt,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //PTP
    factory.queryPTPTask = function (legalEntity, custNum, custName, siteUseId, status, dateF, dateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/ptpquery?legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + '&status=' + status + "&dateF=" + dateF + "&dateT=" + dateT,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportPTP = function (legalEntity, custNum, custName, siteUseId, status, dateF, dateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/exportptp?legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + '&status=' + status + "&dateF=" + dateF + "&dateT=" + dateT,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //PTP Detail
    factory.queryPTPDetailTask = function (id, successcb){
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/queryPTPDetailTask?id=' + id,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //Dispute
    factory.queryDisputeTask = function (legalEntity, custNum, custName, siteUseId, status, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/disputequery?legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + '&status=' + status,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //Remindding
    factory.queryTaskRemindding = function (legalEntity, custNum, custName, siteUseId, dateF, dateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/remindingquery?legalEntity=' + legalEntity + '&custNum=' + custNum + '&custName=' + custName + '&siteUseId=' + siteUseId + "&dateF=" + dateF + "&dateT=" + dateT,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //Dispute Detail
    factory.queryDisputeDetailTask = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/queryDisputeDetailTask?id=' + id,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.newTask = function (deal, legalEntity, customerNo, siteUseId, startDate, taskType, taskContent, taskStatus, isAuto, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/newtask?deal=' + deal + '&legalEntity=' + legalEntity + '&custNum=' + customerNo + '&siteUseId=' + siteUseId + '&startDate=' + startDate + '&taskType=' + taskType + '&taskContent=' + taskContent + '&taskStatus=' + taskStatus + '&isAuto=' + isAuto,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveTask = function (taskId, startDate, taskType, taskContent, taskStatus, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/updatetask?taskId=' + taskId + '&startDate=' + startDate + '&taskType=' + taskType + '&taskContent=' + taskContent + '&taskStatus=' + taskStatus ,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveTaskPMT = function (customerNum, siteUseId, invoiceNum, status, comments, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/saveTaskPMT?customerNum=' + customerNum + '&siteUseId=' + siteUseId + '&invoiceNum=' + invoiceNum + '&statuS=' + status + '&comments=' + comments,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveTaskPTP = function (id, status, comments, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/saveTaskPTP?Id=' + id + '&status=' + status + '&comments=' + comments,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveTaskDispute = function (id, status, comments, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/saveTaskDispute?Id=' + id + '&status=' + status + '&comments=' + comments,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.gettaskPMTSendList = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/gettaskPMTSendList?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.gettaskSOASendList = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/gettaskSOASendList?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.sendTaskByUser = function (templeteLanguage, deal, region, eid, periodId, alertType, toTitle, toName, ccTitle, customerNum, responseDate, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/task/sendTaskByUser?templeteLanguage=' + templeteLanguage + '&deal=' + deal + '&region=' + region +
                '&eid=' + eid + '&periodId=' + periodId + '&alertType=' + alertType + '&toTitle=' + toTitle + '&toName=' + toName + '&ccTitle=' + ccTitle +
                '&customerNum=' + customerNum + '&responseDate=' + responseDate,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
