angular.module('app.invoice', [])
    .config(['crudRouteProvider', function (crudRouteProvider) {
        crudRouteProvider.routesFor('invoice', 'app/collection', 'collection')
            .whenList();
    } ])

    .controller('invoiceListCtrl', ['$scope', 'invoiceService', function($scope, invoiceService) { 
        

    } ]);