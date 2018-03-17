
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
    
    var hourlySeasonality = false;
    var dailySeasonality = false;
    var symbol = '';
    var selectedGroup ='';
    var dataHours = 0;
    var periods = 0;
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
    hourlySeasonality = $('#seasonality-houly').is(':checked');
    dailySeasonality = $('#seasonality-daily').is(':checked');
    periods = $('input[name=period]:checked').val();
    
    postData = {
        symbol: symbol,
        dataHours: dataHours,
        periods: periods,
        hourlySeasonality: hourlySeasonality,
        dailySeasonality: dailySeasonality
    };
    
    $.ajax({
        url: utils.domain + '/Index',
        type:'Post',
        data: postData,
        success: function (data) {
            
            if (data){
                var $table = data.table;
                var $content = '';
                for(var i = 0; i < $table.length; i++)
                {
                    $content += "<tr><td>" +
                        $table[i].id+"</td><td>"+
                        $table[i].ds+"</td><td>"+
                        $table[i].yhat+"</td><td>"+
                        $table[i].yhatLower+"</td><td>"+
                        $table[i].yhatUpper+"</td></tr>";
                }
                $('#table-content').html($content);
            }

            var imgForecast = $('<img />', {
                id: 'forecast',
                src: data.forecastPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            
            var imgComponents = $('<img />', {
                id: 'components',
                src: data.componentsPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            
            $('#asset-name').html(data.assetName);
            $('#forecast-place').html(imgForecast);
            
            
            $('#components-place').html(imgComponents);
            
            utils.loaderHide();
        },
        error: function (error) {
            alert(error.responseJSON.message);
            utils.loaderHide();
        }
    })
});

$('#trigger-block').click(function (){
   $('#use-buttons').click();
});


$('#custom-slider:text').on('input', function(){
    $('#use-slider').click();
    var $val = this.value;
    $('#ex13').slider('setValue', $val);
    
});

$('#ex13').on('change',function(){
    $('#use-slider').click();
});

$('#python-test').click(function () {

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