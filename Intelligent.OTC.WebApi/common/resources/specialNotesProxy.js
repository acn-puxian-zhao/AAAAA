angular.module('resources.specialNotesProxy', []);
angular.module('resources.specialNotesProxy').factory('specialNotesProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('specialNotes');

    factory.forNotes = function (customerCode, successcb, failedcb) {
        return factory.query({ customerCode: customerCode }, successcb, failedcb);
    };


    factory.saveNotes = function (list, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/specialNotes',
            method: 'POST',
            data: list
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });

    }

    return factory;
}]);
