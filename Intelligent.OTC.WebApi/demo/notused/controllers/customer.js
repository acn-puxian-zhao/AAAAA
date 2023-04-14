angular.module('app.customer', [])
    .config(['crudRouteProvider', function (crudRouteProvider) {
        crudRouteProvider.routesFor('customer', 'app/collection', 'collection')
            .whenList();
    }])

    .controller('customerListCtrl', ['$scope', 'customerService', 'modalService', '$location', function ($scope, customerService, modalService, $location) {

        function init() {
            customerService.all(function (list) {
                $scope.custs = list;
            }, function (error) {
                alert(error);
            });
        }
        defaultParams = [];
        $scope.onScoreClicked = function () {

            customerService.getCustomerScore($scope.selectedCust.riskScore).then(function (res) {
                $scope.customerScore = res.data;

                var modalDefaults = { templateUrl: 'app/views/Collection/CustomerScore.html' };

                var modalOptions = {
                    customerScore: $scope.customerScore,
                    headerText: 'Customer Score Details'
                };

                modalService.showModal(modalDefaults, modalOptions).then(function (result) {
                    // something you want to do after commit.

                });

            }, function (error) {
                alert(error);
            });
        }

        $scope.selectedCusts = [];
        $scope.$watchCollection('selectedCusts', function () {
            $scope.selectedCust = angular.copy($scope.selectedCusts[0]);
        })

        $scope.collectionGrid = {
            data: 'custs',
            multiSelect: false,
            selectedItems: $scope.selectedCusts,
            enableColumnResize: false,
            columnDefs: [
            { field: 'customerCode', displayName: 'First Name', width: '25%' },
            { field: 'customerName', displayName: 'Last Name', width: '25%' },
            { field: 'customerClass', displayName: 'Email', width: '25%' },
            { field: 'riskScore', displayName: 'Mobile Number', width: '25%', cellTemplate: '<div class="ngCellText"><a ng-click="onScoreClicked()">{{row.getProperty(col.field)}}</a></div>' },
            { field: 'operations', displayName: 'Operations', width: '25%',
                cellTemplate: '<div class="ngCellText">'
                + '<div><button type="button" class="btn btn-primary" ng-click="onEditClicked()">Edit</button>' +
                +'<button type="button" class="btn btn-primary" ng-click="onProcessClicked()">Process</button></div>' +
                +'</div>'
            }
        ]}

        $scope.onProcessClicked = function () {
            $location.path('/AR Aging List/AR Aging Details');
        }

        $scope.onEditClicked = function () {

            //        $location.path('/customerEdit');
            //        return;

            var modalDefaults = {
                templateUrl: 'app/views/Collection/customerEditForm.html'
            };

            var modalOptions = {
                customer: $scope.selectedCust,
                headerText: 'Customer Details'
            };

            modalService.showModal(modalDefaults, modalOptions).then(function (result) {
                // something you want to do after commit.
                alert('onsave clicked');
            });

        }

        $scope.searchCollection = function () {
            query = new Query('And');
            customerService.complexQuery(
                query.addCondition('CustomerCode', 'Equal', $scope.custCode)
                     .addCondition('CustomerName', 'Equal', $scope.custName)
                     .addCondition('CustomerClass', 'Equal', $scope.custClass)
            //                     .addCondition('CustomerClass', 'Equal', $scope.invNum)
                     .addCondition('State', 'Equal', $scope.status)
                     .orderBy('RiskScore', 'true'), function (list) {
                         $scope.custs = list;
                     }, function (error) {
                         alert(error);
                     });
        }

        init();
    } ]);