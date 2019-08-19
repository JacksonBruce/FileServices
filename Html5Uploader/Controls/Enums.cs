using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html5Uploader.Controls
{
    public enum SettingsNames
    {
        placeholder
        ,
        multiple
        ,
        accept
        ,
        types
        ,
        timeout
        ,
        maxQueue
        ,
        dragable
        ,
        dragContainer
        ,
        progress
        ,
        blobSize
        ,
        sliced
        ,
        limitSize
        ,
        url
        ,
        parseResult


    }

    public enum ClientEventsNames
    {
        selecting 
        ,
        validate
        ,
        selected
        ,
        upload
        ,
        createProgress 
        ,
        getResumableInfoHandler 
        ,
        progress
        ,
        complete 
        ,
        success
        ,
        error
        ,
        cancel
        ,
        proceed
        ,
        pause
        ,
        drop
        ,
        dragover
        ,
        dragleave
       
    }

    public enum UploaderSliceds
    {
        Auto = 0, Enabled = 1, Disabled = 2
    }
}
