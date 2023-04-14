angular.module('app.contact', [])
    .config(['crudRouteProvider', '$routeProvider', function (crudRouteProvider, $routeProvider) {

        var customerCode = ['$route', function ($route) {
            return $route.current.params.customerCode;
        } ];

        crudRouteProvider
        .routesFor('contact', 'customer', 'customer/:customerCode')
            .whenList({
                customerCode: customerCode,
                conts: ['contactProxy', '$route', function (contactProxy, $route) {
                    return contactProxy.forCustomer($route.current.params.customerCode);
                } ]
            })
            .whenEdit({
                customerCode: customerCode,
                cont: ['contactProxy', '$route', function (contactProxy, $route) {
                    return contactProxy.getById($route.current.params.itemId);
                } ]
            })
            .whenNew({
                customerCode: customerCode,
                cont: ['contactProxy', '$route', function (contactProxy, $route) {
                    return new contactProxy();
                } ]
            });
    } ])

    .controller('contactListCtrl', ['$scope', 'crudListExtensions', 'conts', 'customerCode', function ($scope, crudListExtensions, conts, customerCode) {
        $scope.conts = conts;

        angular.extend($scope, crudListExtensions('/customer/' + customerCode + '/contact'));

    } ])

    .controller('contactEditCtrl', ['$scope', 'cont', 'customerCode', function ($scope, cont, customerCode) {
        $scope.cont = cont;

    } ]);

