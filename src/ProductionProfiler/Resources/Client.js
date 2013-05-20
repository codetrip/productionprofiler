
(function ($) { $.fn.CenterIt = function (options) { var defaults = { ignorechildren: true }; var settings = $.extend({}, defaults, options); var control = $(this); control.show(); $(document).ready(function () { CenterItem(); }); $(window).resize(function () { CenterItem(); }); function CenterItem() { var controlHeight = 0; var controlWidth = 0; if (settings.ignorechildren) { controlHeight = control.height(); controlWidth = control.width(); } else { var children = control.children(); for (var i = 0; i < children.length; i++) { if (children[i].style.display != 'none') { controlHeight = children[i].clientHeight; controlWidth = children[i].clientWidth; } } } var controlMarginCSS = control.css("margin"); var controlPaddingCSS = control.css("padding"); if (controlMarginCSS != null) { controlMarginCSS = controlMarginCSS.replace(/auto/gi, '0'); controlMarginCSS = controlMarginCSS.replace(/px/gi, ''); controlMarginCSS = controlMarginCSS.replace(/pt/gi, ''); } var totalMargin = ""; if (controlMarginCSS != "" && controlMarginCSS != null) { totalMargin = controlMarginCSS.split(' '); } var horizontalMargin = 0; var verticalMargin = 0; if (totalMargin != "NaN") { if (totalMargin.length > 0) { horizontalMargin = parseInt(totalMargin[1]) + parseInt(totalMargin[3]); verticalMargin = parseInt(totalMargin[2]) + parseInt(totalMargin[2]); } } if (controlPaddingCSS != null) { controlPaddingCSS = controlPaddingCSS.replace(/auto/gi, '0'); controlPaddingCSS = controlPaddingCSS.replace(/px/gi, ''); controlPaddingCSS = controlPaddingCSS.replace(/pt/gi, ''); } var totalPadding = ""; if (controlPaddingCSS != "" && controlPaddingCSS != null) { totalPadding = controlPaddingCSS.split(' '); } var horizontalPadding = 0; var verticalPadding = 0; if (totalPadding != "NaN") { if (totalPadding.length > 0) { horizontalPadding = parseInt(totalPadding[1]) + parseInt(totalPadding[3]); verticalPadding = parseInt(totalPadding[2]) + parseInt(totalPadding[2]); } } if (verticalMargin == "NaN" || isNaN(verticalMargin)) { verticalMargin = 0; } if (verticalPadding == "NaN" || isNaN(verticalPadding)) { verticalPadding = 0; } var windowHeight = $(window).height(); var windowWidth = $(window).width(); if ($.browser.msie && $.browser.version.substr(0, 1) < 7) { control.css("position", "absolute"); } else { control.css("position", "fixed"); } control.css("height", controlHeight + "px"); control.css("width", controlWidth + "px"); control.css("top", ((windowHeight - (controlHeight + verticalMargin + verticalPadding)) / 2) + "px"); control.css("left", ((windowWidth - (controlWidth + horizontalMargin + horizontalPadding)) / 2) + "px"); } } })(jQuery);

(function ($) {

    $.fn.jsonviewer = function (settings) {
        var config =
        {
            'type_prefix': false,
            'json_name': 'unknown',
            'json_data': null,
            'ident': '12px',
            'inner-padding': '2px',
            'outer-padding': '4px',
            'debug': false
        };

        if (settings)
            $.extend(config, settings);

        this.each(function (key, element) {
            format_value(element, config['json_name'], config['json_data'], config, true);
        });

        return this;

    };

    function format_value(element, name, data, config, init) {
        var v = new TypeHandler(data);
        var container = $('<div/>');
        container.appendTo(element);
        container.addClass('ui-widget').css({ 'padding': config['outer-padding'], 'padding-left': config['ident'] });
        var header = $('<div/>');
        header.appendTo(container);
        header.addClass('ui-widget-header').css({ 'text-align': 'left', 'white-space': 'nowrap', 'overflow': 'hidden' });

        if (init)
            header.append('<a style="font-size:10px;" href="javascript:$.viewengine.hideJsonObject();">CLOSE WINDOW</a>');
        else
            header.text('' + name);

        var content;
        if (v.type() === "object" || v.type() === "array") {
            content = $('<div/>');
            content.appendTo(container);
            content.addClass('ui-widget-content').css({ 'overflow': 'hidden', 'white-space': 'nowrap', 'padding': config['inner-padding'] });
            for (name in data) {
                format_value(content, name, data[name], config, false);
            }
        }
        else {
            content = $('<div/>');
            content.appendTo(container);
            content.addClass('ui-widget-content').css({ 'overflow': 'hidden', 'white-space': 'nowrap' });

            if (data !== null && v.type() === "string" && data.indexOf("Date") > -1)
                content.text('' + new Date(parseInt(data.substr(6))).toUTCString());
            else
                content.text('' + data);
        }
    };


    // number, boolean, string, object, array, date
    function TypeHandler(value) {
        this._type = this.get_type(value);
    };

    TypeHandler.prototype.type = function () { return this._type; };

    TypeHandler.prototype.get_type = function (value) {
        var base_type = typeof value;
        var result = "unsupported";
        switch (base_type) {
            case "number": result = base_type; break;
            case "string": result = base_type; break;
            case "boolean": result = base_type; break;
            case "object":
                if (Number.prototype.isPrototypeOf(value)) { result = "number"; break; }
                if (String.prototype.isPrototypeOf(value)) { result = "string"; break; }
                if (Date.prototype.isPrototypeOf(value)) { result = "date"; break; }
                if (Array.prototype.isPrototypeOf(value)) { result = "array"; break; }
                if (Object.prototype.isPrototypeOf(value)) { result = "object"; break; }
        }
        return result;
    };
})(jQuery);

var currentHtml;
var jQueryProfiler = {};

var lastId = 0;
var nextId = function () {
    return ++lastId;
};

if (window.jQueryProfiler) {

    (function () {

        $.profiler = {};
        $.viewengine = {};

        $.extend($.profiler, {
            update: function () {

            },
            formatDate: function (jsonDate) {
                return jsonDate == null ? '' : new Date(parseInt(jsonDate.substr(6))).toUTCString();
            },
            emptyIfNull: function (val, postfix) {
                return val === null || val === 0 ? '' : val + postfix;
            }
        });

        $.extend($.viewengine, {
            container: null,
            title: null,
            html: null,
            init: function (data) {
                this.container = $('#profiler');
                this.title = $('#title');
            },
            attachEvents: function () {
                this.container.delegate('.delete', 'click', function (e) {
                    if (!window.confirm("Are you sure you want to delete this item?")) {
                        e.preventDefault();
                        return false;
                    }
                });
                $('body').delegate('.closePopUp', 'click', function (e) {
                    $(this).closest('.popup').remove();
                });
            },
            attachGenericEvents: function () {
                this.container.delegate('.confirm', 'click', function (e) {
                    if (!confirm('Are you sure you wish to do this?')) {
                        e.preventDefault();
                        return false;
                    }
                });
            },
            attachLongRequestEvents: function () {
                $('.profileThis').click(function (evt) {
                    var data = document[$(this).attr('id')];
                    var popUpHtml =
                        '<div id="profileUrlPopUp" class="popup"><form method="post" action="/profiler?handler=apr">' +
                            '<p>Server (or leave blank): <input type="text" name="server" value="@@SERVER@@" /></p>' +
                            '<p>URL (supports regex): <input type="text" name="url" value="@@URL@@" /></p>' +
                            '<p>Profile Count: <input type="text" name="ProfilingCount" value="5" /></p>' +
                            '<p><input type="submit" value="Profile This URL" /><button type="button" class="closePopUp">Cancel</button></p>' +
                            '</form></div>';
                    $('body').append(popUpHtml.replace('@@SERVER@@', data.server).replace('@@URL@@', data.url));
                }
                );
            },
            attachDetailEvents: function () {
                this.container.find("tr.togglechild").click(function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var currentRow = $(this);
                    var row = currentRow.next("tr");
                    row.toggleClass("hidden");

                    var padding = parseInt(currentRow.attr("data-padding"));

                    if (row.hasClass("hidden")) {
                        currentRow.find('td:first').attr("style", 'cursor:pointer; background: url(/profiler?resource=Plus.gif&contenttype=image/gif) ' + (padding - 10) + 'px 7px no-repeat; padding-left:' + padding + 'px');
                    } else {
                        currentRow.find('td:first').attr("style", 'cursor:pointer; background: url(/profiler?resource=Minus.gif&contenttype=image/gif) ' + (padding - 10) + 'px 7px no-repeat; padding-left:' + padding + 'px');
                    }
                });
                this.container.find("div.rh, div.rh-nested, div.rh-nested-selected").click(function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var css, cssSelected;
                    var div = $(this);
                    var table = div.next('table:first');

                    table.toggleClass("hidden");

                    if (div.hasClass("rh") || div.hasClass("rh-selected")) {
                        css = "rh";
                        cssSelected = "rh-selected";
                    } else {
                        css = "rh-nested";
                        cssSelected = "rh-nested-selected";
                    }

                    if (table.hasClass("hidden")) {
                        div.removeClass(cssSelected);
                        div.addClass(css);
                    } else {
                        div.removeClass(css);
                        div.addClass(cssSelected);
                    }
                });
                this.container.find("a.dataitemjson").click(function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    $this = $(this);
                    var data;
                    if ($this.hasClass("ret")) {
                        data = $.parseJSON($this.closest('tr').find('input.returnvalue').val()); //THIS IS NOT WORKING IN IE!!!
                    } else if ($this.hasClass("arg")) {
                        data = $.parseJSON($this.closest('tr').find('input.arg' + $this.attr("id")).val()); //THIS IS NOT WORKING IN IE!!!
                    } else {
                        data = $.parseJSON($this.next('input').val()); //THIS IS NOT WORKING IN IE!!!
                    }
                    $.viewengine.showJsonObject(data);
                });
            },
            showJsonObject: function (data) {
                var wrapper = this.container.find('div#itemcontainer');
                wrapper.jsonviewer({ json_name: '', json_data: data });
                wrapper.toggle();
                wrapper.CenterIt();
            },
            hideJsonObject: function (data) {
                var wrapper = this.container.find('div#itemcontainer');
                wrapper.empty();
                wrapper.toggle();
            },
            renderDataItem: function (item) {
                if (item.Format === 1) { //json
                    return "<div class='dataitemvalue'><a class='dataitemjson' href='#'>view item</a><input type='hidden' value='" + item.Value.replace(new RegExp("'", 'g'), "") + "' /></div>";
                } else if (item.Format === 2) { //xml
                    return item.Value;
                } else {
                    return item.Value;
                }
            },
            getItemByType: function (item) {
                if (item.Format === 1) { //json
                    return item.Value.replace(new RegExp("'", 'g'), "");
                } else if (item.Format === 2) { //xml
                    return item.Value;
                } else {
                    return item.Value;
                }
            },
            renderUrlToProfiles: function (data) {
                currentHtml = '<form action="/profiler?handler=apr" method="post">' +
                '<table class="w1000">' +
                '<tr><th>Url to profile (Supports Regular Expressions)</th><th>Server</th><th>Profile Count</th><th>Threshold For Recording (ms)</th><th></th></tr>' +
                '<tr><td><input id="Url" name="Url" style="width:555px" type="text" value="" /></td>' +
                '<td><input name="Server" style="width:200px" type="text" value="" /></td>' +
                '<td><input name="ProfilingCount" maxlength="4" style="width:75px" type="text" value="" /></td>' +
                '<td><input name="ThresholdForRecordingMs" maxlength="4" style="width:75px" type="text" value="" /></td>' +
                '<td><input type="submit" value="Add" class="btn" /></td></tr>' +
                '</table></form>';

                if (data.Data.length > 0) {
                    currentHtml += '<table class="w1000"><tr><th>Enable</th><th>Url</th><th>Server</th><th>Profile Count</th><th>Threshold For Recording (ms)</th><th>Delete</th><th>Update</th></tr>';

                    //these forms broken in Firefox
                    $.each(data.Data, function (idx, itm) {
                        var profilingCount = itm.ProfilingCount === null ? '' : itm.ProfilingCount;
                        var checked = itm.Enabled ? 'checked="checked"' : '';
                        var bgcolor = itm.Enabled ? 'style="background-color:#F5D0A9"' : '';
                        currentHtml += '<tr ' + bgcolor + '><form action="/profiler?handler=upr" method="post">' +
                        '<input name="Url" type="hidden" value="' + itm.Url + '" />' +
                        '<td><input name="Enabled" type="checkbox" ' + checked + ' value="true" /><input name="Enabled" type="hidden" value="false" /></td>' +
                        '<td>' + itm.Url + '</td>' +
                        '<td><input name="Server" style="width:150px" type="text" value="' + $.profiler.emptyIfNull(itm.Server, '') + '" /></td>' +
                        '<td><input name="ProfilingCount" style="width:50px" type="text" value="' + profilingCount + '" /></td>' +
                        '<td><input name="ThresholdForRecordingMs" style="width:50px" type="text" value="' + (itm.ThresholdForRecordingMs || '') + '" /></td>' +
                        '<td><input type="submit" value="Delete" name="Delete" class="btn delete" /></td>' +
                        '<td><input type="submit" value="Update" name="Update" class="btn" /></td>' +
                        '</form></tr>';
                    });

                    currentHtml += $.viewengine.renderPaging(data.Paging, 9, '/profiler?handler=vpr');
                    currentHtml += '</table>';
                } else {
                    currentHtml += '<div class="noresults">No URLs are currently being profiled, either turn on automatic monitoring in the profiler configuration or manually add URLs you want to profile using the form above.</div>';
                }
                this.container.html(currentHtml);
                this.renderHeading("Profiled URLs");
                this.attachEvents();
            },
            renderLongUrls: function (data) {
                currentHtml = "";
                data.Paging.baseUrl = '/profiler?handler=longrequests&action=viewlongrequests';
                this.renderTable(
                    ['Url', 'Server', 'Timestamp', 'Duration', 'Actions'],
                    'Long Running Urls',
                    data.Data,
                    '',
                    '',
                    null,
                    function (itm) { return [itm.UrlPathAndQuery, itm.Server, itm.FriendlyRequestLocal, itm.DurationMs, { __buttons: [{ __cssClass: 'profileThis', url: itm.UrlPathAndQuery, server: itm.Server, text: 'Profile'}]}]; },
                    data.Paging
                );
                currentHtml += '<form action="/profiler?handler=clearlongrequests" method="post"><input type="submit" value="Clear All Requests" class="confirm"/></form>';
                this.container.html(currentHtml);
                this.renderHeading('Long running requests');
                this.attachEvents();
                this.attachLongRequestEvents();
            },
            renderResults: function (data) {
                currentHtml = '<table class="w800"><tr><th>Url</th><th>Delete</th></tr>';

                $.each(data.Data, function (idx, itm) {
                    currentHtml += '<form action="/profiler?handler=dprurl" method="post"><input type="hidden" name="Url" value="' + itm.Url + '" />' +
                    '<tr><td><a href="/profiler?handler=results&action=previewresults&url=' + itm.EncodedUrl + '">' + itm.Url + '</a></td>' +
                    '<td width="75px"><input type="submit" class="btn delete" value="Delete" /></td></tr></form>';
                });

                currentHtml += $.viewengine.renderPaging(data.Paging, 2, '/profiler?handler=results&action=results');
                currentHtml += '</table>';
                this.container.html(currentHtml);
                this.renderHeading("Profiled Requests Results");
                this.attachEvents();
            },
            renderResultsPreview: function (data) {
                currentHtml = '<table class="w1000"><tr><th>Url</th><th>CapturedOnUtc</th><th>ElapsedMilliseconds</th><th>Server</th><th>Delete</th></tr>';

                var encodedUrl;
                $.each(data.Data, function (idx, itm) {
                    encodedUrl = itm.EncodedUrl;
                    currentHtml += '<form action="/profiler?handler=dprid&url=' + itm.EncodedUrl + '" method="post"><input type="hidden" name="Id" value="' + itm.Id + '" />' +
                    '<tr><td><a href="/profiler?handler=results&action=resultsdetail&id=' + itm.Id + '">' + itm.Url + '</a></td>' +
                    '<td>' + $.profiler.formatDate(itm.CapturedOnUtc) + '</td><td>' + itm.ElapsedMilliseconds + '</td><td>' + itm.Server + '</td>' +
                    '<td><input type="submit" class="btn delete" value="Delete" /></td></tr></form>';
                });

                currentHtml += $.viewengine.renderPaging(data.Paging, 5, '/profiler?handler=results&action=previewresults&url=' + encodedUrl);
                currentHtml += '</table>';
                this.container.html(currentHtml);
                this.renderHeading("Profiled Requests Results");
                this.attachEvents();
            },
            renderResultsDetail: function (data) {
                var responseUrl = '<a target="_blank" href="/profiler?handler=response&id=' + data.Id + '">view response</a>';
                currentHtml = '<div id="itemcontainer" class="itemcontainer"></div><table class="heading"><tr><th>Url</th><th>Request Id</th><th>Response</th><th>Captured On</th><th>Server</th><th>Elapsed Milliseconds</th><th>Client IP</th><th>Session Id</th><th>Session Key</th><th>Samplng Id</th><th>Ajax</th></tr>' +
                '<tr><td><a href="/profiler?handler=results&action=previewresults&url=' + data.EncodedUrl + '">' + data.Url + '</a></td><td>' + data.Id + '</td><td>' + responseUrl + '</td><td>' + $.profiler.formatDate(data.CapturedOnUtc) + '</td><td>' + data.Server + '</td><td>' + data.ElapsedMilliseconds + 'ms</td><td>' + data.ClientIpAddress + '</td><td>' + data.SessionId + '</td><td>' + data.SessionUserId + '</td><td>' + data.SamplingId + '</td><td>' + data.Ajax + '</td></tr></table>';

                if (data.Methods.length > 0) {
                    $.viewengine.renderTable(["Method", "Elapsed", "Started", "Stopped", "Errors", "Messages"], "Method Info", data.Methods, "rh", "heading hidden", function (method) {
                        $.viewengine.renderMethodInfo(method, 0);
                    });
                }

                if (data.RequestData.length > 0) {
                    $.each(data.RequestData, function (idx, itm) {
                        $.viewengine.renderTable(["Name", "Value"], itm.Name, itm.Data, "rh", "heading hidden", function (dataItem) {
                            currentHtml += '<tr><td>' + dataItem.Name + '</td><td>' + dataItem.Value + '</td></tr>';
                        });
                    });
                }

                if (data.ResponseData.length > 0) {
                    $.each(data.ResponseData, function (idx, itm) {
                        $.viewengine.renderTable(["Name", "Value"], itm.Name, itm.Data, "rh", "heading hidden", function (dataItem) {
                            currentHtml += '<tr><td>' + dataItem.Name + '</td><td>' + dataItem.Value + '</td></tr>';
                        });
                    });
                }

                this.container.html(currentHtml);
                this.renderHeading("Profiled Request Details");
                this.attachDetailEvents();
            },
            getMethodInfoHiddenFields: function (method) {
                var html = '';
                if (method.ReturnValue !== null) {
                    html += "<input type='hidden' class='returnvalue' value='" + method.ReturnValue.Value.replace(new RegExp("'", 'g'), "") + "' />";
                }

                if (method.Arguments !== null) {
                    $.each(method.Arguments, function (idx, itm) {
                        html += "<input type='hidden' class='arg" + idx + "' value='" + itm.Value.replace(new RegExp("'", 'g'), "") + "' />";
                    });
                }
                return html;
            },
            renderMethodInfo: function (method, level) {
                var padding = ((level * 25) + 15);
                var hasLogMessages = method.Messages && method.Messages.length;
                var hasExceptions = method.Exceptions && method.Exceptions.length;
                var hasData = method.Data && method.Data.length;
                var enableToggle = hasLogMessages || hasExceptions || hasData;
                var rowClass = enableToggle ? 'class="togglechild"' : '';
                var css = enableToggle ? 'style="cursor:pointer; background: url(/profiler?resource=Plus.gif&contenttype=image/gif) ' + (padding - 10) + 'px 7px no-repeat; padding-left:' + padding + 'px"' : 'style="padding-left:' + padding + 'px"';

                currentHtml += '<tr ' + rowClass + ' data-padding="' + padding + '">' + $.viewengine.getMethodInfoHiddenFields(method) + '<td ' + css + '>&nbsp;' + $.viewengine.renderMethodSignature(method) + '</td><td>' + method.ElapsedMilliseconds + 'ms</td><td>' + method.StartedAtMilliseconds + 'ms</td><td>' + method.StoppedAtMilliseconds + 'ms</td><td>' + method.Exceptions.length + '</td><td>' + method.Messages.length + '</td></tr>';

                if (hasLogMessages || hasExceptions || hasData) {
                    currentHtml += '<tr class="hidden"><td style="padding-left:' + (padding + 5) + 'px" colspan="6">';

                    if (hasLogMessages) {
                        $.viewengine.renderTable(["Logged at", "Logger", "Level", "Error"], "Messages", method.Messages, "rh-nested-selected", "nested", function (message) {
                            currentHtml += '<tr><td style="width:80px">' + message.Milliseconds + 'ms</td><td style="width:80px">' + message.Logger + '</td><td style="width:100px">' + message.Level + '</td><td>' + message.Message + '</td></tr>';
                        });
                    }

                    if (hasExceptions) {
                    	$.viewengine.renderTable(["Logged at", "Exception Type", "Message"], "Exceptions", method.Exceptions, "rh-nested-selected", "nested", function (exception) {
                            currentHtml += '<tr><td style="width:80px">' + exception.Milliseconds + 'ms</td><td style="width:250px">' + exception.Type + '</td><td>' + exception.Message.replace(new RegExp('\n', 'g'), '<br />') + '</td></tr>';
                        });
                    }

                    if (hasData) {
                        $.each(method.Data, function (idx, itm) {
                        	$.viewengine.renderTable(["Name", "Value", "Type"], itm.Name, itm.Data, "rh-nested-selected", "nested", function (dataItem) {
                                currentHtml += '<tr><td>' + dataItem.Name + '</td><td>' + $.viewengine.renderDataItem(dataItem) + '</td><td>' + dataItem.Type + '</td></tr>';
                            });
                        });
                    }

                    currentHtml += '</td></tr>';
                }

                if (method.Methods.length > 0) {
                    $.each(method.Methods, function (idx, innerMethod) {
                        $.viewengine.renderMethodInfo(innerMethod, level + 1);
                    });
                }
            },
            renderMethodSignature: function (method) {
                var html = '';
                if (method.ReturnValue !== null) {
                    html += "<a class='dataitemjson ret' href='#'>Return Value</a> ";
                }
                html += method.MethodName;
                if (method.Arguments !== null) {
                    var argLength = method.Arguments.length - 1;
                    html += '(';
                    $.each(method.Arguments, function (idx, itm) {
                        html += "<a class='dataitemjson arg' id=" + idx + " href='#'>Argument " + (idx + 1) + "</a>";
                        if (idx < argLength)
                            html += ', ';
                    });
                    html += ')';
                }
                return html;
            },
            renderHeading: function (title) {
                var html = '<div style="padding:5px 0px 15px 0px"><h1>' + title + '</h1><a class="heading" href="/profiler?handler=vpr">URLs to Profile</a><a class="heading" href="/profiler?handler=results&action=results">Profiler Results</a><a class="heading" href="/profiler?handler=longrequests&action=viewlongrequests">Long-running Requests</a><a class="heading" href="/profiler?handler=cfg&action=viewcfg">Configuration</a></div>';
                this.title.html(html);
            },
            renderTable: function (layout, heading, data, divCss, tableCss, render, getTds, paging) {
                if (data.length > 0) {
                    currentHtml += '<div class="' + divCss + '">' + heading + '</div><table class="' + tableCss + '"><tr>';
                    for (var head in layout) {
                        currentHtml += '<th>' + layout[head] + '</th>';
                    }
                    currentHtml += '</tr>';

                    render = render || this.getDefaultRenderTds(getTds).bind(this);

                    var colCount = 0;
                    $.each(data, function (idx, itm) {
                        var itemColCount = render(itm);
                        if (itemColCount > colCount)
                            colCount = itemColCount;
                    });

                    if (paging) {
                        currentHtml += $.viewengine.renderPaging(paging, colCount, paging.baseUrl);
                    }

                    currentHtml += '</table>';
                }
            },

            getDefaultRenderTds: function (getTds) {
                return function (itm) {
                    var colCount = 0;
                    currentHtml += "<tr>";
                    $.each(getTds(itm), function (idx, td) {
                        currentHtml += "<td>";
                        if (td.__buttons) {
                            $.each(td.__buttons, function (idx, button) {
                                var id = "btn_" + nextId();
                                currentHtml += '<button class="' + button.__cssClass + '" id="' + id + '">' + button.text + '</button>';
                                document[id] = button;
                                colCount++;
                            });
                        }
                        else {
                            colCount++;
                            currentHtml += td;
                        }
                        currentHtml += "</td>";
                    });
                    currentHtml += '</tr>';
                    return colCount;
                };
            },

            renderPaging: function (pagingInfo, colspan, url) {
                var html = '<tr><td class="pagingcell" colspan="' + colspan + '"><div class="paging"><div class="prevpage">';
                html += pagingInfo.HasPreviousPage ? '<a href="' + url + '&pgno=' + (pagingInfo.PageNumber - 1) + '">previous</a>' : 'previous';
                html += '</div><div class="items">showing ' + pagingInfo.FirstItem + ' to ' + pagingInfo.LastItem + ' of ' + pagingInfo.TotalItems + ' items</div>';
                html += '<div class="pages">page ' + pagingInfo.PageNumber + ' of ' + pagingInfo.TotalPages + '</div><div class="nextpage">';
                html += pagingInfo.HasNextPage ? '<a href="' + url + '&pgno=' + (pagingInfo.PageNumber + 1) + '">next</a>' : 'next';
                html += '</div></div></td></tr>';
                return html;
            },
            renderConfiguration: function (config) {
                var html = '<form action="/profiler?handler=cfg&action=setcfg" method="post">' +
                    '<table class="w1000">' +
                    '<tr><th>Configuration Property</th><th>Current Value</th></tr>';

                for (var key in config) {
                    html += '<tr><td>' + key + '</td><td><input name="cfg-' + key + '" style="width:250px" type="text" value="' + config[key] + '" /></td></tr>';
                }

                if (location.href.indexOf('s=1') !== -1) {
                    html += '<tr><td colspan="2"><b>Configuration settings were updated successfully.</b></td></tr>';
                }

                html += '<tr><td colspan="2"><input type="submit" value="Update" class="btn" /></td></tr></table></form>';

                this.container.html(html);
                this.renderHeading("Profiler Configuration");
            }
        });

        $(document).ready(function () {
            $.viewengine.init();

            switch (profileAction) {
                case "results":
                    {
                        $.viewengine.renderResults(profileData);
                        break;
                    }
                case "previewresults":
                    {
                        $.viewengine.renderResultsPreview(profileData);
                        break;
                    }
                case "resultsdetail":
                    {
                        $.viewengine.renderResultsDetail(profileData.Data);
                        break;
                    }
                case "viewcfg":
                    {
                        $.viewengine.renderConfiguration(profileData.Data);
                        break;
                    }
                case "viewlongrequests":
                    {
                        $.viewengine.renderLongUrls(profileData);
                        break;
                    }
                default:
                    {
                        $.viewengine.renderUrlToProfiles(profileData);
                        break;
                    }
            }
            $.viewengine.attachGenericEvents();
        });

    })(jQueryProfiler);
}  
