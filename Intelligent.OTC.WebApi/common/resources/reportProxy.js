angular.module('resources.reportProxy', []);
angular.module('resources.reportProxy').factory('reportProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('reports');

    factory.getODStatistics = function (successcb) {
            $http({
                url: APPSETTING['serverUrl'] + '/api/reportod/statistics',
                method: 'GET',
            }).then(function (result) {
                successcb(result.data);
            }).catch(function (result) {
                alert(result.data);
            });
    }

    factory.getODDetails = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportod/details?page=' + page + "&pageSize=" + pageSize,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadODReport = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportod/download',
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackStatistics = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/statistics',
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getNotFeedbackList = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/notfeedback?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getHasFeedbackList = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/hasfeedback?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackDetails = function (sDate, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/details?sDate=' + sDate + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackHistoryList = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/getfeedbackhistory?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadFeedbackReport = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/download',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadFeedbackDetail = function (sDate, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedback/detaildownload?sDate=' + sDate,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackStatisticsByCs = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedbackbycs/statistics',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackDetailsByCs = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedbackbycs/details?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadFeedbackReportByCs = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedbackbycs/download',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackStatisticsBySales = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedbackbysales/statistics',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFeedbackDetailsBySales = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedbackbysales/details?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadFeedbackReportBySales = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportfeedbackbysales/download',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getUnApplyStatistics = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportunapply/statistics',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getUnApplyDetails = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportunapply/details?page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadUnApplyReport = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportunapply/download',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPTPStatistics = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportptp/statistics',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPTPDetails = function (page, pageSize, category, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportptp/details?page=' + page + "&pageSize=" + pageSize + "&category=" + category,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadPTPReport = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/reportptp/download',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
