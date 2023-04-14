angular.module('resources.caHisDataProxy', []);
angular.module('resources.caHisDataProxy').factory('caHisDataProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('caHisData');
        
    factory.getBankHisDataDetails = function (statusselect, legalEntity, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDateT, createDateF, createDateT, bsType,page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaBankStatementList?ishistory=0&statusselect=' + statusselect + '&legalEntity=' + legalEntity + '&transNumber=' + transNumber +
                '&transcurrency=' + transcurrency + '&transamount=' + transamount + '&transCustomer=' + transCustomer + '&transaForward=' + transaForward +
                '&valueDataF=' + valueDataF + '&valueDataT=' + valueDateT + '&createDateF=' + createDateF + '&createDateT=' + createDateT + '&bsType=' + bsType + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getBankHisHisDataDetails = function (statusselect, legalEntity, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDateT, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaBankStatementList?ishistory=1&statusselect=' + statusselect + '&legalEntity=' + legalEntity + '&transNumber=' + transNumber +
                '&transcurrency=' + transcurrency + '&transamount=' + transamount + '&transCustomer=' + transCustomer + '&transaForward=' + transaForward +
                '&valueDataF=' + valueDataF + '&valueDataT=' + valueDateT + '&createDateF=' + '&createDateT=' +'&bsType=' + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getIdentifyHisDataDetails = function (taskId, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getIdentifyList?taskId=' + taskId + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getAdvisorHisDataDetails = function (taskId, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getAdvisortList?taskId=' + taskId + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPmtDetailHisDataDetails = function (groupNo, legalEntity, customerNum, currency, amount, transactionNumber, invoiceNum, valueDateF, valueDateT, createDateF, createDateT, isClosed, hasBS, hasMatched, hasINV, page, pageSize, successcb) {
        $http({
            //url: 'common/resources/cajson/reHisDataList.json?page=' + page + "&pageSize=" + pageSize,
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaPmtDetailList?groupNo=' + groupNo + '&legalEntity=' + legalEntity + '&customerNum=' + customerNum + '&currency=' + currency + '&amount=' + amount + 
                '&transactionNumber=' + transactionNumber + '&invoiceNum=' + invoiceNum + '&valueDateF=' + valueDateF + '&valueDateT=' + valueDateT + '&createDateF=' + createDateF + '&createDateT=' + createDateT +
                '&hasBS=' + hasBS + '&hasMatched=' + hasMatched + '&hasinv=' + hasINV + '&isClosed=' + isClosed + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    
    factory.getCaPmtDetailListByBsId = function (bsId, successcb) {
        $http({
            //url: 'common/resources/cajson/reHisDataList.json?page=' + page + "&pageSize=" + pageSize,
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaPmtDetailListByBsId?bsId=' + bsId,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.changePMTBsId = function (bsId, pmtId, successcb) {
        $http({
            //url: 'common/resources/cajson/reHisDataList.json?page=' + page + "&pageSize=" + pageSize,
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/changePMTBsId?bsId=' + bsId + '&pmtId=' + pmtId,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deletePmtBsByBsId = function (bsId, successcb) {
        $http({
            //url: 'common/resources/cajson/reHisDataList.json?page=' + page + "&pageSize=" + pageSize,
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/deletePmtBsByBsId?bsId=' + bsId,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPmtDetailHisDataBsById = function (reconid, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaPmtBsList?reconid=' + reconid,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPmtDetailHisDataDetailById = function (reconid, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaPmtDetailDetailList?reconid=' + reconid,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }; 

    factory.getPtpHisDataDetails = function (customerNum, legalEntity, customerCurrency, invCurrency, amt, localAmt, ptpDateF, ptpDateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caPTPController/getCaPTPList?customerCurrency=' + customerCurrency + '&customerNum=' + customerNum + '&legalEntity=' + legalEntity + '&invCurrency=' + invCurrency + '&amt=' + amt + '&localAmt=' + localAmt + "&ptpDateF=" + ptpDateF + "&ptpDateT=" + ptpDateT,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getTaskDataDetails = function (taskType, status, taskName, dateF, dateT, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caTaskController/getCaTaskList?taskType=' + taskType + '&status=' + status + '&taskName=' + taskName + '&dateF=' + dateF + '&dateT=' + dateT + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getTaskDataDetailsByType = function (page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caTaskController/getCaTaskListByType?page=' + page + "&pageSize=" + pageSize + "&taskType=1,2,8",
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.getAgentCustomerDataDetails = function (page, pageSize, entity, type, successcb) {


        var url = APPSETTING['serverUrl'] + '/api/caBankStatementController/likeAgentCustomer?page=' + page + "&pageSize=" + pageSize + "&bankid=" + entity.id;


        if (type !== 1) {
            // all
            url = APPSETTING['serverUrl'] + '/api/caBankStatementController/allAgentCustomer?page=' + page + "&pageSize=" + pageSize + "&legalEntity=" + entity.legalEntity;
        }
        $http({
            url: url,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

 
 

    factory.getPaymentCustomerDataDetails = function (page, pageSize, entity, type, successcb) {


        var url = APPSETTING['serverUrl'] + '/api/caBankStatementController/likePaymentCustomer?page=' + page + "&pageSize=" + pageSize + "&bankid=" + entity.id;

        if (type !== 1) {
            // all
            url = APPSETTING['serverUrl'] + '/api/caBankStatementController/allPaymentCustomer?page=' + page + "&pageSize=" + pageSize + "&legalEntity=" + entity.legalEntity;
        }

        $http({
            url: url,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
 

 
 

 
 

    factory.getArHisDataDetails = function (entity, type, successcb) {

        if (type !== 1) {
            url = APPSETTING['serverUrl'] + '/api/caBankStatementController/ArHisDataDetails?customerNum=' + entity.customerNum + '&legalEntity=' + entity.legalEntity;
        } else {
            url = APPSETTING['serverUrl'] + '/api/caBankStatementController/reconArHisDataDetails?reconId=' + entity.reconId;
        };

        $http({
            url: url,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getArDatas = function (entity, type, successcb) {

        url = APPSETTING['serverUrl'] + '/api/caBankStatementController/ArHisDataDetails?customerNum=' + entity.customerNum + '&legalEntity=' + entity.legalEntity;

        $http({
            url: url,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    
    factory.getReconDetails = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/getReconGroupList?taskId=' + id,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getReconDetailsMultipleResult = function (bsIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/getReconGroupMultipleResultListByBSIds',
            method: 'POST',
            data: { "bsIds": bsIds }
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.changeToMatch = function (bsId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/changeToMatch',
            method: 'POST',
            data: { "bsIds": bsId }
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getReconBankDetails = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/getUnmatchBankList?taskId=' + id,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getReconDetailsByBSIds = function (bsIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/getReconGroupListByBSIds',
            method: 'POST',
            data: { "bsIds": bsIds }
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getReconBankDetailsByBSIds = function (bsIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/getUnmatchBankListByBSIds',
            method: 'POST',
            data: { "bsIds": bsIds }
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getReconArDetails = function (customerList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/getArListByCustomerNum',
            method: 'POST',
            data: customerList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.groupExport = function (customerList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/groupExport',
            method: 'POST',
            data: customerList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.bankRowSave = function (dto, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/updateBank',
            method: 'POST',
            data: dto
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.identifyCustomer = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/identifyCustomer',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.unknownCashAdvisor = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/unknownCashAdvisor',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.pmtUnknownCashAdvisor = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/pmtUnknownCashAdvisor',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.recon = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/recon',
            method: 'POST',
            data: bankIds
        })
            .then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetFileFromWebApi = function (path, successcb) {
        var pathgroup = [];
        pathgroup.push(path);
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetFileFromWebApi',
            method: 'POST',
            data: pathgroup,
            responseType: 'arraybuffer'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetFileFromWebApiById = function (path, successcb) {
        var pathgroup = [];
        pathgroup.push(path);
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetFileFromWebApiById',
            method: 'POST',
            data: pathgroup,
            responseType: 'arraybuffer'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetFileByFileId = function (fileId, successcb) {
        var pathgroup = [];
        pathgroup.push(path);
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetFileByFileId',
            method: 'POST',
            data: pathgroup,
            responseType: 'arraybuffer'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveBankStatement = function (bank, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/saveBank',
            method: 'POST',
            data: bank
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deleteBankStatement = function (bank, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/deleteBank',
            method: 'POST',
            data: bank
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.revertBankStatement = function (bank, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/revert',
            method: 'POST',
            data: bank
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.isExistedTransactionNum = function (bankId, transactionNum, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/isExistedTransactionNum?bankId=' + bankId + "&transactionNum=" + transactionNum,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.ungroup = function (reconIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/ungroup',
            method: 'POST',
            data: reconIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.group = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/group',
            method: 'POST',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.changeNeedSendMail = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/needSendMail',
            method: 'POST',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.changeNeedSendMailAll = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/allNeedSendMail',
            method: 'POST',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPaymentCustomer = function (bankid, successcb) {


        var url = APPSETTING['serverUrl'] + '/api/unknownAdjustment/likePaymentCustomer?bankid=' + bankid;

        $http({
            url: url,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.SendMails = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/sendMail',
            method: 'POST',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.uploadPMTByFileId = function (fileId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/UploadPMTNoFile?fileId=' + fileId,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getBankByTaskId = function (taskId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetBankByTaskId?taskId=' + taskId,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportBankStatement = function (statusselect, legalEntity, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDateT, createDateF, createDateT, bsType) {
        window.location = APPSETTING['serverUrl'] + '/api/caBankStatementController/exporBankStatementAll?ishistory=0&statusselect=' + statusselect + '&legalEntity=' + legalEntity + '&transNumber=' + transNumber +
            '&transcurrency=' + transcurrency + '&transamount=' + transamount + '&transCustomer=' + transCustomer + '&transaForward=' + transaForward +
            '&valueDataF=' + valueDataF + '&valueDataT=' + valueDateT + '&createDateF=' + createDateF + '&createDateT=' + createDateT + '&bsType=' + bsType;
    };

    factory.exportPmtDetail = function (groupNo, legalEntity, customerNum, currency, amount, transactionNumber, invoiceNum, valueDateF, valueDateT, createDateF, createDateT, isClosed, hasBS, hasMatched, hasINV) {
        window.location = APPSETTING['serverUrl'] + '/api/caBankStatementController/exporPmtDetail?groupNo=' + groupNo + '&legalEntity=' + legalEntity + '&customerNum=' + customerNum + '&currency=' + currency + '&amount=' + amount +
            '&transactionNumber=' + transactionNumber + '&invoiceNum=' + invoiceNum + '&valueDateF=' + valueDateF + '&valueDateT=' + valueDateT + '&createDateF=' + createDateF + '&createDateT=' + createDateT +
            '&hasBS=' + hasBS + '&hasMatched=' + hasMatched + '&hasinv=' + hasINV + '&isClosed=' + isClosed;
    };


    factory.exporReconResultByReconId = function (taskId) {
        window.location = APPSETTING['serverUrl'] + '/api/caReconController/exporReconResultByReconId?taskId=' + taskId;
    };

    factory.exporReconResultByBankIds = function (bsIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caReconController/exporReconResultByBsIds',
            method: 'POST',
            data: { "bsIds": bsIds }
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getBankByTranc = function (transactionNum, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetBankByTranc?transactionNum=' + transactionNum,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getBankCharge = function (customerNum, legalEntity, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAttribute/getBankCharge?customerNum=' + customerNum + '&legalEntity=' + legalEntity,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deletePmtBs = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/pmt/deletePmtBs?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.autoRecon = function (statusselect, legalEntity, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDateT, createDateF, createDateT, bsType, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/autoRecon?ishistory=0&statusselect=' + statusselect + '&legalEntity=' + legalEntity + '&transNumber=' + transNumber +
                '&transcurrency=' + transcurrency + '&transamount=' + transamount + '&transCustomer=' + transCustomer + '&transaForward=' + transaForward + '&valueDataF=' + valueDataF + '&valueDataT=' + valueDateT + '&createDateF=' + createDateF + '&createDateT=' + createDateT + '&bsType=' + bsType,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.ignore = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/ignore',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.unlock = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/unlock',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.batchDelete = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/batchDelete',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.doExportUnknownDataByIds = function (bankList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/doExportUnknownDataByIds',
            method: 'POST',
            data: bankList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getCaMailAlertListbybsid = function (bsid, alertType, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaMailAlertListbybsid?bsid=' + bsid + '&alertType=' + alertType+ '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.cancelCaMailAlertbyid = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/cancelCaMailAlertbyid?id=' + id,
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
