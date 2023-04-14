angular.module('constants.message', []);
angular.module('constants.message')
    .constant('I18N.MESSAGES',
    { 'crud.customer.save.success': 'A customer was added successfully' })
    //    .constant('APPSETTING',
    //        { 'serverUrl': 'http://vrtdlcws2001:8090/Site', 'ignoreAuth':'false', 'loginUrl':'http://vrtdlcws2001:8090/Portal/User/Logout', 'xcceleratorUrl':'http://vrtdlcws2001:8090/Portal', 'changePassword':'/User/ChangePassword' })
    .constant('APPSETTING',
        {
            'serverUrl': 'http://localhost:55209/',
        'ignoreAuth': 'true',
        'loginUrl': 'http://vrtdlcws2001:8090/Portal/User/Logout',
        'xcceleratorUrl': 'http://vrtdlcws2001:8090/Portal',
        'changePassword': '/User/ChangePassword',
        'jobStartTime': '23:59:00',
        'jobEndTime': '23:59:01',
        'jobStartTimeAfternoon': '15:15:00',
        'jobEndTimeAfternoon': '16:15:00'
        
    })
    ;