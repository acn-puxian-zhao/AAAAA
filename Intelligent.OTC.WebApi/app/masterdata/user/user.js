angular.module('app.masterdata.user', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        .when('/user/init', {
            templateUrl: 'app/masterdata/user/user-init.tpl.html',
            controller: 'userInitCtrl',
            resolve: {
                //首次加载第一页
                //                currentDeal: ['periodProxy', function (periodProxy) {
                //                    return periodProxy.searchInfo("Deal");
                //                } 
                //                ]
            }
        });
    } ])

    .controller('userInitCtrl', ['baseDataProxy', '$scope', 'permissionProxy', 'APPSETTING', 'collectorSignatureProxy', 'mailAccountProxy',
        function (baseDataProxy, $scope,
            permissionProxy, APPSETTING, collectorSignatureProxy, mailAccountProxy) {
            $scope.$parent.helloAngular = "OTC - User initialization";

        $scope.hidflg = false;

        baseDataProxy.SysTypeDetails('013', function (list)
        {
            angular.forEach(list, function (r) {
                $scope.languageList = r["013"];
                $scope.selectedLanguage = $scope.languageList[0].detailValue;
                //加载collectorSingature
                collectorSignatureProxy.getCollectortSign($scope.languageList[0].detailValue, function (collect) {
                    $scope.collector = collect;
                });
            });
            
            
        }, function (res)
        {
            alert(res);
            });

        $scope.changeLanguage = function ()
        {
            collectorSignatureProxy.getCollectortSign($scope.selectedLanguage, function (collect) {
                $scope.collector = collect;
            });
        }

        mailAccountProxy.getMailAccount(function (mailAccount) {
            if (mailAccount !== undefined && mailAccount !==null)
            $scope.userName = mailAccount.userName;
        }, function (res) { alert(res) });

        permissionProxy.getCurrentUser("dummy", function (currUser) {
            if (!currUser.email) {
                alert("User's email account did not setup in intelligent OTC application.");
                $scope.canGrant = false;
            }
            $scope.email = currUser.email;
            $scope.canGrant = true;
        });

        $scope.changePassWordURL = APPSETTING.xcceleratorUrl + APPSETTING.changePassword;
        $scope.authPath = 'https://accounts.google.com/o/oauth2/auth?client_id=358145595498-1odj75ja10ujagebt3rsv743l8v4cbd2.apps.googleusercontent.com&redirect_uri=urn:ietf:wg:oauth:2.0:oob&scope=https://mail.google.com&response_type=code';
        $scope.grant = function () {
            baseDataProxy.initialUser($scope.authCode, $scope.email, function (res) {
                alert('Permisson grant successed.');
            }, function (errMsg) {
                alert(errMsg);
            });
        }

        
        //保存签名
        $scope.saveAutograph = function () {
            var ss = [];
            ss.push($scope.collector.signature);
            ss.push($scope.selectedLanguage);
           // var ss = encodeURIComponent($scope.collector.signature);
            collectorSignatureProxy.addOrUpdateCollect(ss, function (res) { alert(res); }, function () { alert("Update Failed"); });
        }

         //$scope.hidflg=true;
        baseDataProxy.getAuthentication(function (auth) {
            $scope.hidflg = auth;
        });

        $scope.saveMailAccount = function () {
            var regex = /^([0-9A-Za-z\-_\.]+)@([0-9a-z]+\.[a-z]{2,3}(\.[a-z]{2})?)$/g;
            if (!regex.test($scope.userName)) {
                alert("Email Format is wrong");
                return;
            }
            var mailAccount = {};
            mailAccount.userName = $scope.userName;
            mailAccount.userid = $scope.oldpwd; //借用UserId字段
            mailAccount.password = $scope.pwd;
            $scope.mailAccount = mailAccount;
            mailAccountProxy.saveMailAccount($scope.mailAccount, function (res) {
                alert(res);
            }, function (errMsg) {
                alert(errMsg);
            });
        };
       
    } ]);