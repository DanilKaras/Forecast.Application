var requests = (function () {
    
    var updateAssets = function () {
        utils.loaderShow();
        $.ajax({
            url: utils.domain + '/UpdateAssets',
            type: 'GET',
            success: function () {
                $('.selectpicker option').remove();
                selecter();
            },
            error: function (error) {
                utils.loaderHide();
                alert(error.statusText);
            }
        });
    };
    
    var selecter = function () {
        $.ajax({
            url: utils.domain + '/SymbolsList',
            type: 'GET',
            success: function (data) {
                if (data.symbols){
                    var picker = $(".selectpicker");
                    var jsonData = data.symbols;
                    for (var i = 0; i < jsonData.length; i++) {
                        picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
                    }
                    picker.selectpicker('refresh');
                }
                utils.loaderHide();
            },
            error: function (error) {
                utils.loaderHide();
            }
        })
    };
    
    
    var sendToServer = function (data) {
        if (data){
            $.ajax({
                url: utils.domain + '/Index',
                type:'Post',
                data: data,
                success: function (data) {
                    onSuccessLoad(data);
                    utils.loaderHide();
                    
                },
                error: function (error) {
                    utils.loaderHide();
                    var message = error.responseJSON.message;
                    if(error.responseJSON.requestCount){
                        message += " Made requests: "+ error.responseJSON.requestCount;
                    }
                    alert(message);
                }
            })
        }
        else{
            toastr.error("Cannot send data to server");
        }
    };
    
    var testPython = function () {
        $.ajax({
            url: utils.domain + '/TestLink',
            type:'Get',
            success: function (data) {
                alert('success!');
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
    };
    return {
        updateAssets: updateAssets,
        selecter: selecter,
        sendToServer: sendToServer,
        testPython: testPython
    };
})();