

function VisibleMenuButton(divId, blAdd, blSave, blEdit, blConfirm, blDelete, blCancel) {

    if (blAdd) $('#' + divId + ' .btnAdd').show(); else $('#' + divId + ' .btnAdd').hide();
    if (blSave) $('#' + divId + ' .btnSave').show(); else $('#' + divId + ' .btnSave').hide();
    if (blEdit) $('#' + divId + ' .btnEdit').show(); else $('#' + divId + ' .btnEdit').hide();
    if (blConfirm) $('#' + divId + ' .btnConfirm').show(); else $('#' + divId + ' .btnConfirm').hide();
    if (blDelete) $('#' + divId + ' .btnDelete').show(); else $('#' + divId + ' .btnDelete').hide();
    if (blCancel) $('#' + divId + ' .btnCancel').show(); else $('#' + divId + ' .btnCancel').hide();

}

function DisableMenuButton(divId, blAdd, blSave, blEdit, blConfirm, blDelete, blCancel) {

    $('#' + divId + ' .btnAdd').prop('disabled', blAdd);
    $('#' + divId + ' .btnSave').prop('disabled', blSave);
    $('#' + divId + ' .btnEdit').prop('disabled', blEdit);
    $('#' + divId + ' .btnConfirm').prop('disabled', blConfirm);
    $('#' + divId + ' .btnDelete').prop('disabled', blDelete);
    $('#' + divId + ' .btnCancel').prop('disabled', blCancel);

}

function SetMenuActionMode(divId, action, objRoleDetail)
{   
    //objRoleDetail = GetUserRoleInfo();
    
    if (objRoleDetail) {

        var blAdd = StringToBoolean(objRoleDetail.IsAdd);
        var blUpd = StringToBoolean(objRoleDetail.IsUpdate);
        var blDel = StringToBoolean(objRoleDetail.IsDelete);
        var blConf = StringToBoolean(objRoleDetail.IsConfirm);

        switch (action) {
            case ReadOnly: // Disable All
                DisableMenuButton(divId, true, true, true, true, true, true);
                break;
          
            case Init:  // init only
                DisableMenuButton(divId, !blAdd, true, !blUpd, !blConf, !blDel, false);
                break;

            case AddOnly:  // Add only
                DisableMenuButton(divId, !blAdd, true, true, true, true, true);
                break;

            case EditOnly:  // EditOnly only
                DisableMenuButton(divId, true, true, !blUpd, true, true, true);
                break;

            case New , Update:  // New Mode/update
                DisableMenuButton(divId,true, false, true, true, true, false);
                break;
            default:

        }
    }
    else // Disable all
    {
        DisableMenuButton(divId, true, true, true, true, true, true);
    }

}