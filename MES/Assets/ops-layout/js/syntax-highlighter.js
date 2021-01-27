(function () {
    var root = this;

    root.jsPlumbSyntaxHighlighter = function (toolkit, selector, exportType) {
        var syntaxHighlight = function (json) {
            json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
            return "<pre>" + json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
                var cls = 'number';
                if (/^"/.test(match)) {
                    if (/:$/.test(match)) {
                        cls = 'key';
                    } else {
                        cls = 'string';
                    }
                } else if (/true|false/.test(match)) {
                    cls = 'boolean';
                } else if (/null/.test(match)) {
                    cls = 'null';
                }
                return '<span class="' + cls + '">' + match + '</span>';
            }) + "</pre>";
        };

        var datasetContainer = document.querySelector(selector);

        var updateDataset = this.updateDataset = function () {
            if (datasetContainer) {
                datasetContainer.innerHTML = syntaxHighlight(JSON.stringify(toolkit.exportData({
                    type: exportType || "json"
                }), null, 4));
            }
        };

        toolkit.bind("dataUpdated", updateDataset);
        toolkit.bind("dataLoadEnd", updateDataset);

        updateDataset();
    }

}).call(this);