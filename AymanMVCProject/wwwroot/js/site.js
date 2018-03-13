
$(document).ready(function(){

    $('#ex13').slider({
        ticks: [0, 240, 480, 720],
        ticks_labels: ['0', '240','480', '720'],
        ticks_snap_bounds: 30
        });

    bindSelect();
});

$('#update-assets').click(function (){
    utils.loaderShow();
    $.ajax({
        url: utils.domain + '/UpdateAssets',
        type: 'GET',
        success: function () {
        $('.selectpicker option').remove();
            bindSelect();
        },
        error: function (error) {
            utils.loaderHide();
            alert(error.statusText);
        }
    });
});


$('#make-forecast').click(function () {
    utils.loaderShow();
    
    var useSeasonality = false;
    var symbol = '';
    var selectedGroup ='';
    var dataHours = 0;
    var postData = '';
    
    selectedGroup = $('input[name=radio]:checked').val();
    
    switch(selectedGroup){
        case utils.group.useButtons:
            dataHours = $('input[name=toggle]:checked').val();
            break;
        case utils.group.useSlider:
            dataHours = $('#ex13').slider('getValue');
            break;
        default:
            break;
    }

    symbol = $('.selectpicker option:selected').val();
    useSeasonality = $('#seasonality-chk').is(':checked');
    
    postData = {
        symbol: symbol,
        dataHours: dataHours,
        useSeasonality: useSeasonality
    };
    
    $.ajax({
        url: utils.domain + '/Index',
        type:'Post',
        data: postData,
        success: function (data) {
            alert('success!');
            utils.loaderHide();
        },
        error: function (error) {
            alert('Not enough historical data!');
            utils.loaderHide();
        }
    })
});

$('#custom-slider:text').on('input', function(){
    var $val = this.value;
    $('#ex13').slider('setValue', $val);
});

var bindSelect = function () {
    $.ajax({
        url: utils.domain + '/SymbolsList',
        type: 'GET',
        success: function (data) {
            var picker = $(".selectpicker");
            var jsonData = data.symbols;
            for (var i = 0; i < jsonData.length; i++) {
                picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
            }
            picker.selectpicker('refresh');
            utils.loaderHide();
        },
        error: function (error) {
            utils.loaderHide();
            
        }
    })
};


