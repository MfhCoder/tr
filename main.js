var liveTimeChanges = $.connection.liveTimeChangesHub;
$(document).ready(function () {
    //liveTimeChanges.on('updateCode', function (code) {
    //    $("#CountryCode").val(parseInt(code));
    //    refreshGrid();
    //});
    liveTimeChanges.on('updateCountryCode', function (code) {
        $("#CountryCode").val(parseInt(code));
        refreshGrid();
    });
    $.connection.hub.start().done(function () {

    });
    $('#FormAdd').ajaxForm({
        beforeSend: function () {
            MainLoader.show();
        },
        success: function (res) {
            if (GetMsgType(res.MsgType) == "success") {
                Toast(res.Title, res.Subject, "success");
                $('#FormAdd')[0].reset();
                liveTimeChanges.server.updateCountryCode();
            }
            else if (GetMsgType(res.MsgType) == "warning") {
                Toast(res.Title, res.Subject, "warning");
            }
            else {
                Toast(res.Title, res.Subject, "error");
            }
            MainLoader.hide();
        }
    });
    initTbl();
});
function refreshGrid() {
    $('#gridLoader').attr('class', 'fa fa-refresh fa-spin');
    var callingUrl = $('#gridBox').data("load");
    $.ajax({
        type: "Get",

        url: callingUrl,
        success: function (res) {
            $('#gridBox').html(res + "");
            $('#gridLoader').attr('class', 'fa fa-flag');
            initTbl();
        }
    });
}
function initTbl() {
    var tbl = $('#datatable').DataTable(tbColumns);
}
$(document).on('click', 'button[data-loadmodal]', function () {
    MainLoader.show();
    var url = $(this).data("loadmodal");
    $.ajax({
        type: "Get",
        url: url,
        success: function (res) {
            var modal = $(res).appendTo('#MainContent').modal('show').on('hidden.bs.modal', function (e) {
                $(this).data('bs.modal', null);
                $(this).remove();
            });
            MainLoader.hide();
            $('#FormEdit').ajaxForm({
                beforeSend: function () {
                    MainLoader.show();
                    modal.modal('hide');
                },
                success: function (res) {
                    if (GetMsgType(res.MsgType) == "success") {
                        Toast(res.Title, res.Subject, "success");
                    }
                    else if (GetMsgType(res.MsgType) == "warning") {
                        Toast(res.Title, res.Subject, "warning");
                    }
                    else {
                        Toast(res.Title, res.Subject, "error");
                    }
                    MainLoader.hide();
                    refreshGrid();
                }
            });
        }
    });
});
$(document).on('click', 'button[data-delete]', function () {
    var deleteUrl = $(this).data('delete');
    Message(swalStrings.deleteText, swalStrings.deleteConfirmMsg, "confirm", function () {
        MainLoader.show();
        $.ajax({
            type: "Get",
            url: deleteUrl,
            success: function (res) {
                if (GetMsgType(res.MsgType) == "success") {
                    Toast(res.Title, res.Subject, "success");
                    liveTimeChanges.server.updateCountryCode();
                }
                else if (GetMsgType(res.MsgType) == "warning") {
                    Toast(res.Title, res.Subject, "warning");
                }
                else {
                    Toast(res.Title, res.Subject, "error");
                }
                MainLoader.hide();
            }
        });
    });
});
