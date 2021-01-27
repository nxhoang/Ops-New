/* jQuery human-task mapping plugin
Version: 1.0
Author: Nguyen Xuan Hoang
Released date: 27-Mar-2020
*/

(function ($) {
    const btnOpEmpRefresh = "btnOpEmpRefresh",
        btnOpEmpSaveChanges = "btnOpEmpSaveChanges",
        btnTopAlignId = "btnTopAlign";

    function getImage(url) {
        return new Promise(function (resolve, reject) {
            const img = new Image();
            img.onload = function () {
                resolve(img);
            };
            img.onerror = function (e) {
                reject(e);
            };
            img.src = url;
        });
    }

    $.fn.humanMapPlugin = function (options) {
        class RectObj {
            constructor(type, x, y, width, height, borderColor, backgroundColor, textColor, isDragging, isShow, data) {
                this.Type = type;
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
                this.BorderColor = borderColor;
                this.BackgroundColor = backgroundColor;
                this.TextColor = textColor;
                this.IsDragging = isDragging;
                this.IsShow = isShow;
                this.Data = data;
            }
        }

        const settings = $.extend({
            DragOk: false,
            CanvasWidth: 1360,
            CanvasHeight: 700,
            ScaleX: 1.2,
            ScaleY: 1.2,
            RectArray: [],
            EmpImgDir: "jquery.human-task-map/img",
            EmpMaleImg: "male-user.png",
            EmpFemaleImg: "female-user.png"
        }, options);

        let ctx, selectedEmp = null, startX, startY, mImg, fImg, isChange = false, isRedRect = false;
        //clickRefreshButton();

        return this.each((i, conDiv) => {
            conDiv.onclick = clickConDiv;
            const canvas = document.createElement("canvas");
            const divCanvas = document.createElement("div");
            divCanvas.classList.add("op-emp-map");

            // appending top controls
            conDiv.appendChild(createTopControls());
            divCanvas.appendChild(canvas);
            conDiv.appendChild(divCanvas);

            ctx = canvas.getContext("2d");

            canvas.width = settings.CanvasWidth;
            canvas.height = settings.CanvasHeight;
            canvas.onmousedown = myDown;
            canvas.onmouseup = myUp;
            canvas.onmousemove = myMove;
            canvas.onZoomIn = onZoomIn;

            conDiv.createRect = (type, x, y, width, height, borderColor, backgroundColor, textColor, isDragging, isShow, data) =>
                new RectObj(type, x, y, width, height, borderColor, backgroundColor, textColor, isDragging, isShow, data);
            conDiv.bindData = bindData;
            conDiv.getData = getData;
            conDiv.draw = draw;
            conDiv.reloadEmp = reloadEmp;
            conDiv.isLayoutChange = () => isChange;
            conDiv.setIsChange = (value) => isChange = value;
            //conDiv.clearCanvas = clear;

            $.when(getImage(`${settings.EmpImgDir}/${settings.EmpMaleImg}`), getImage(`${settings.EmpImgDir}/${settings.EmpFemaleImg}`)).done((i1, i2) => {
                mImg = i1;
                fImg = i2;
                draw();
            });
        });

        function createTopControls() {
            const ctrCon = document.createElement("div"),
                btnRefresh = document.createElement("button"),
                btnSave = document.createElement("button"),
                btnTopAlign = document.createElement("button");

            ctrCon.classList.add("op-emp-map__div-agc");
            ctrCon.appendChild(btnRefresh);
            ctrCon.appendChild(btnSave);

            btnRefresh.setAttribute("id", btnOpEmpRefresh);
            btnRefresh.setAttribute("type", "button");
            btnRefresh.setAttribute("title", "Refresh layout");
            btnRefresh.appendChild(createRefreshIconSvg());

            btnSave.setAttribute("id", btnOpEmpSaveChanges);
            btnSave.setAttribute("type", "button");
            btnSave.disabled = true;
            btnSave.setAttribute("title", "Save changes");
            btnSave.appendChild(createSaveIconSvg());

            btnTopAlign.setAttribute("id", btnTopAlignId);
            btnTopAlign.setAttribute("type", "button");
            btnTopAlign.setAttribute("title", "Align Top");
            btnTopAlign.appendChild(createTopAlignIconSvg());

            // adding clicking events
            clickRefreshButton(btnRefresh);
            clickSaveButton(btnSave);
            clickTopAlignButton(btnTopAlign);

            return ctrCon;
        }

        function createRefreshIconSvg() {
            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg"),
                path1 = document.createElementNS("http://www.w3.org/2000/svg", "path"),
                path2 = document.createElementNS("http://www.w3.org/2000/svg", "path");

            svg.setAttribute("viewbox", "0 0 20 20");
            svg.setAttribute('width', '20px');
            svg.setAttribute('height', '20px');

            path1.setAttribute("d", "M 10.210938 2.53125 L 10.210938 0 L 5.613281 4.601562 L 10.210938 9.199219 L 10.210938 6.664062 C 15.59375 6.214844 19.066406 9.792969 19.941406 14.496094 C 20.507812 7.664062 16.121094 3.625 10.210938 2.53125 Z M 10.210938 2.53125 ");
            path1.setAttribute("fill", "#ffffff");

            path2.setAttribute("d", "M 9.890625 17.441406 L 9.890625 20 L 14.539062 15.351562 L 9.890625 10.703125 L 9.890625 13.265625 C 5.574219 13.625 1.199219 11.46875 0.0585938 5.347656 C -0.515625 12.269531 3.941406 16.339844 9.890625 17.441406 Z M 9.890625 17.441406 ");
            path2.setAttribute("fill", "#ffffff");

            svg.appendChild(path1);
            svg.appendChild(path2);

            return svg;
        }

        function createSaveIconSvg() {
            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg"),
                path1 = document.createElementNS("http://www.w3.org/2000/svg", "path"),
                path2 = document.createElementNS("http://www.w3.org/2000/svg", "path");

            svg.setAttribute("viewbox", "0 0 20 20");
            svg.setAttribute('width', '20px');
            svg.setAttribute('height', '20px');

            path1.setAttribute("d", "M 19.867188 3.769531 L 16.230469 0.132812 C 16.144531 0.046875 16.03125 0 15.910156 0 L 1.816406 0 C 0.816406 0 0 0.816406 0 1.816406 L 0 18.183594 C 0 19.183594 0.816406 20 1.816406 20 L 18.183594 20 C 19.183594 20 20 19.183594 20 18.183594 L 20 4.089844 C 20 3.96875 19.953125 3.855469 19.867188 3.769531 Z M 4.546875 0.910156 L 14.546875 0.910156 L 14.546875 6.363281 C 14.546875 6.863281 14.136719 7.273438 13.636719 7.273438 L 5.453125 7.273438 C 4.953125 7.273438 4.546875 6.863281 4.546875 6.363281 Z M 16.363281 19.089844 L 3.636719 19.089844 L 3.636719 10.910156 L 16.363281 10.910156 Z M 19.089844 18.183594 C 19.089844 18.683594 18.683594 19.089844 18.183594 19.089844 L 17.273438 19.089844 L 17.273438 10.453125 C 17.273438 10.203125 17.070312 10 16.816406 10 L 3.183594 10 C 2.929688 10 2.726562 10.203125 2.726562 10.453125 L 2.726562 19.089844 L 1.816406 19.089844 C 1.316406 19.089844 0.910156 18.683594 0.910156 18.183594 L 0.910156 1.816406 C 0.910156 1.316406 1.316406 0.910156 1.816406 0.910156 L 3.636719 0.910156 L 3.636719 6.363281 C 3.636719 7.367188 4.453125 8.183594 5.453125 8.183594 L 13.636719 8.183594 C 14.640625 8.183594 15.453125 7.367188 15.453125 6.363281 L 15.453125 0.910156 L 15.722656 0.910156 L 19.089844 4.277344 Z M 19.089844 18.183594 ");
            path1.setAttribute("fill", "#ffffff");

            path2.setAttribute("d", "M 11.363281 6.363281 L 13.183594 6.363281 C 13.433594 6.363281 13.636719 6.160156 13.636719 5.910156 L 13.636719 2.273438 C 13.636719 2.019531 13.433594 1.816406 13.183594 1.816406 L 11.363281 1.816406 C 11.113281 1.816406 10.910156 2.019531 10.910156 2.273438 L 10.910156 5.910156 C 10.910156 6.160156 11.113281 6.363281 11.363281 6.363281 Z M 11.816406 2.726562 L 12.726562 2.726562 L 12.726562 5.453125 L 11.816406 5.453125 Z M 11.816406 2.726562 ");
            path2.setAttribute("fill", "#ffffff");

            svg.appendChild(path1);
            svg.appendChild(path2);

            return svg;
        }

        function createTopAlignIconSvg() {
            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg"),
                path1 = document.createElementNS("http://www.w3.org/2000/svg", "path");

            svg.setAttribute("viewbox", "0 0 20 20");
            svg.setAttribute('width', '20px');
            svg.setAttribute('height', '20px');

            path1.setAttribute("d", "M 9.449219 3.054688 L 2.066406 3.054688 L 2.066406 20.109375 L 9.449219 20.109375 Z M 8.6875 19.34375 L 2.832031 19.34375 L 2.832031 3.816406 L 8.6875 3.816406 Z M 18.738281 3.054688 L 11.359375 3.054688 L 11.359375 13.042969 L 18.742188 13.042969 L 18.742188 3.054688 Z M 17.976562 12.28125 L 12.121094 12.28125 L 12.121094 3.816406 L 17.976562 3.816406 Z M 0.222656 1.335938 L 0.222656 0 L 19.886719 0 L 19.886719 1.335938 Z M 0.222656 1.335938 ");
            path1.setAttribute("fill", "#ffffff");

            svg.appendChild(path1);

            return svg;
        }

        function clickRefreshButton(btnRefresh) {
            btnRefresh.addEventListener("click", (e) => {
                if (settings.onRefreshCanvas) settings.onRefreshCanvas(e, isChange);
            });
        }

        function clickSaveButton(btnSave) {
            btnSave.addEventListener("click", (e) => {
                if (settings.onSaveCanvas) settings.onSaveCanvas(e, settings.RectArray);
            });
        }

        function clickTopAlignButton(btnTopAlign) {
            btnTopAlign.addEventListener("click", () => {
                console.log("align top");
            });
        }

        function getData() {
            return settings.RectArray;
        }

        // Drawing arrow
        function drawArrow(p1, p2, size) {
            ctx.save();
            const points = getEdges(p1, p2);

            if (points.length < 2) return;

            p1 = points[0], p2 = points[points.length - 1];

            // Rotate the context to point along the path
            const dx = p2.x - p1.x,
                dy = p2.y - p1.y,
                len = Math.sqrt(dx * dx + dy * dy);
            //ctx.scale(settings.ScaleX, settings.ScaleY);
            ctx.translate(p2.x, p2.y);
            ctx.rotate(Math.atan2(dy, dx));

            // line
            ctx.lineCap = "round";
            ctx.beginPath();
            ctx.moveTo(0, 0);
            ctx.lineTo(-len, 0);
            ctx.closePath();
            ctx.strokeStyle = "#3b3b3b";
            ctx.stroke();

            // arrow head
            ctx.beginPath();
            ctx.moveTo(0, 0);
            ctx.lineTo(-size, -size);
            ctx.lineTo(-size, size);
            ctx.closePath();

            ctx.fillStyle = "#3b3b3b";
            ctx.fill();

            ctx.restore();
        }

        // Find all transparent/opaque transitions between two points
        // Uses http://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        function getEdges(p1, p2, cutoff) {
            if (!cutoff) cutoff = 220; // alpha threshold

            const dx = Math.abs(p2.x - p1.x),
                dy = Math.abs(p2.y - p1.y),
                sx = p2.x > p1.x ? 1 : -1,
                sy = p2.y > p1.y ? 1 : -1,
                x0 = Math.min(p1.x, p2.x),
                y0 = Math.min(p1.y, p2.y),
                pixels = ctx.getImageData(x0, y0, dx + 1, dy + 1).data,
                hits = [];
            var over = null;

            for (let x = p1.x, y = p1.y, e = dx - dy; x !== p2.x || y !== p2.y;) {
                const alpha = pixels[((y - y0) * (dx + 1) + x - x0) * 4 + 3];
                if (over !== null && (over ? alpha < cutoff : alpha >= cutoff)) {
                    hits.push({
                        x: x,
                        y: y
                    });
                }
                const e2 = 2 * e;
                if (e2 > -dy) {
                    e -= dy;
                    x += sx;
                }
                if (e2 < dx) {
                    e += dx;
                    y += sy;
                }
                over = alpha >= cutoff;
            }
            return hits;
        }

        // Drawing a single employee rectangle
        function rect(r) {
            // Drawing user image
            const x = r.X,
                y = r.Y;

            if (r.Data.ImageUrl === null || r.Data.ImageUrl === undefined || r.Data.ImageUrl.trim() === "") {
                if (r.Data.Gender && r.Data.Gender === "Male") {
                    ctx.drawImage(mImg, x + 20, y + 2, 60, 60);
                } else {
                    ctx.drawImage(fImg, x + 20, y + 2, 60, 60);
                }
            } else {
                //const imgUser = new Image();
                //imgUser.onload = () => {
                //    ctx.drawImage(imgUser, x + 20, y + 2, 60, 60);
                //};
                //imgUser.src = r.Data.ImageUrl;
                //console.log(r.Data.UserImg);
                //console.log(r.Data);

                if (r.Data.UserImg) ctx.drawImage(r.Data.UserImg, x + 20, y + 2, 60, 60);
                //console.log(r.Data.UserImg);
            }

            // drawing text
            ctx.font = "8px Comic Sans MS";
            ctx.fillStyle = r.TextColor;
            let name = "",
                employeeCode = "",
                department = "",
                position = "";
            if (r && r.Data) {
                if (r.Data.Name) name = r.Data.Name;
                if (r.Data.EmployeeCode) employeeCode = r.Data.EmployeeCode;
                if (r.Data.Department) department = r.Data.Department;
                if (r.Data.Position) position = r.Data.Position;
            }
            ctx.fillText(name, x + 5, y + 72);
            ctx.fillText(`Id: ${employeeCode}`, x + 5, y + 81);
            ctx.fillText(`Dept: ${department}`, x + 5, y + 90);
            ctx.fillText(`Pos: ${position}`, x + 5, y + 99);
            //ctx.fillText(`Birthday: ${r.Data.Birthday}`,  x + 5, y + 108);
            //ctx.fillText(`Line: ${r.Data.Line}`, x + 5, y + 117);
        }

        function wrapText(context, text, x, y, maxWidth, lineHeight) {
            const words = text.split(" ");
            let line = "";

            for (let i = 0; i < words.length; i++) {
                const testLine = line + words[i] + " ",
                    metrics = context.measureText(testLine),
                    testWidth = metrics.width;
                if (testWidth > maxWidth && i > 0) {
                    context.fillText(line, x, y);
                    line = words[i] + " ";
                    y += lineHeight;
                } else {
                    line = testLine;
                }
            }
            context.fillText(line, x, y);
        }

        // Drawing process rectangle
        function rectPrc(r) {
            //console.log(r);

            // drawing text
            ctx.font = "13px Times New Roman";
            ctx.fillStyle = r.TextColor;

            const x = r.X,
                y = r.Y;

            //console.log(r);

            // Drawing text by wrapping
            wrapText(ctx, `[${r.Data.OpNum}] ${cutOffOpNameStr(r.Data.OpName, 89, 86)}`, x + 5, y + 12, 95, 12);

            // If there is an employee that was assigned to process, draw brief information employee
            if (r.Data.Emp) {
                // Drawing user image
                if (r.Data.Emp.ImageUrl === null || r.Data.Emp.ImageUrl === undefined || r.Data.Emp.ImageUrl.trim() === "") {
                    if (r.Data.Emp.Gender && r.Data.Emp.Gender === "Male") {
                        ctx.drawImage(mImg, x + 1, y + 79, 40, 40);
                    } else {
                        ctx.drawImage(fImg, x + 1, y + 79, 40, 40);
                    }
                } else {
                    //const imgUser = new Image();
                    //imgUser.onload = () => {
                    //    //console.log(imgUser);
                    //    ctx.drawImage(imgUser, x + 1, y + 79, 40, 40);
                    //};
                    //imgUser.src = r.Data.Emp.ImageUrl;
                    ctx.drawImage(r.Data.Emp.UserImg, x + 1, y + 79, 40, 40);
                    //console.log(r.Data);
                }

                // drawing text
                ctx.font = "8px Comic Sans MS";
                ctx.fillStyle = r.TextColor;

                const n = r.Data.Emp.Name ? cutOffStr(r.Data.Emp.Name, 12) : "",
                    pos = r.Data.Emp.Position ? cutOffStr(r.Data.Emp.Position, 12) : "",
                    eCode = r.Data.Emp.EmployeeCode ? r.Data.Emp.EmployeeCode : "";

                ctx.fillText(n, x + 36, y + 95);
                ctx.fillText(`Id: ${eCode}`, x + 36, y + 105);
                ctx.fillText(pos, x + 36, y + 115);
            }
        }

        // If string is so long, cutoff it as abbreviation name (ex: TRẦN T B LÀNH to TRẦN LÀNH)
        function cutOffStr(str, max) {
            if (str.length <= max) return str;
            const res = str.split(" "),
                rs1 = `${res[0]} ${res[res.length - 1]}`,
                rs2 = rs1.length > max ? res[0] : rs1;
            return rs2.length > max ? cutOffOpNameStr(rs2, max, max - 3) : rs2;
        }

        // If length of operation name is greater than 95, cutoff it and plus ...
        function cutOffOpNameStr(opName, max, lastIndex) {
            return opName.length > max ? `${opName.substring(0, lastIndex)}...` : opName;
        }

        function draw() {
            clear(); // Clearing area of draggable rectangles.

            const arrowFlows = [];

            // Looping the array then draw rectangles.
            for (let r of settings.RectArray) {
                if (r.IsShow) {
                    //console.log(r.X);
                    //r.X += settings.ScaleX;
                    //console.log(r.X);
                    //r.Y *= settings.ScaleY;

                    ctx.strokeStyle = r.BorderColor;
                    ctx.fillStyle = r.BackgroundColor;
                    ctx.lineWidth = 1;
                    ctx.fillRect(r.X, r.Y, r.Width, r.Height);
                    //ctx.scale(1, 1);
                    //ctx.strokeRect(r.X, r.Y, r.Width, r.Height);

                    ctx.strokeRect(r.X, r.Y, r.Width, r.Height);

                    if (r.Type === "emp") {
                        // Drawing employee rectangle
                        rect(r);
                    } else {
                        // Drawing process rectangle
                        rectPrc(r);

                        // Getting start and end points for flows
                        if (r.Data.NextOp) {
                            const nextPrc = settings.RectArray.filter(x => x.Type === "prc" && x.Data.OpSerial.toString() === r.Data.NextOp);

                            if (nextPrc.length > 0) {
                                arrowFlows.push({
                                    Start: {
                                        x: r.X + 50, // width/2
                                        y: r.Y + 60 // height/2
                                        //x: r.X * settings.ScaleX + 50, // width/2
                                        //y: r.Y * settings.ScaleY + 60 // height/2
                                    },
                                    End: {
                                        x: nextPrc[0].X + 50,
                                        y: nextPrc[0].Y + 60
                                        //x: nextPrc[0].X * settings.ScaleX + 50,
                                        //y: nextPrc[0].Y * settings.ScaleY + 60
                                    },
                                    Size: 5
                                });
                            }
                        }
                    }
                }
            }

            // Looping of array to draw arrow line.
            for (var a of arrowFlows) {
                //console.log(a);
                drawArrow(a.Start, a.End, a.Size);
            }
        }

        // clear the canvas
        function clear() {
            ctx.clearRect(0, 0, settings.CanvasWidth, settings.CanvasHeight);
        }

        // Intersecting rectangles
        function intersectRect(r1, r2) {
            return !(r2.left > r1.right || r2.right < r1.left || r2.top > r1.bottom || r2.bottom < r1.top);
        }

        // Moving event
        function myMove(e) {
            //console.log(e.clientX);
            //console.log(e.clientY);

            // if we're dragging anything...
            //if (settings.DragOk) {
            // tell the browser we're handling this mouse event
            e.preventDefault();
            e.stopPropagation();

            // get the current mouse position
            const bc = this.getBoundingClientRect(),
                mx = parseInt(e.clientX - bc.left),
                my = parseInt(e.clientY - bc.top),
                dx = mx - startX, // calculate the distance the mouse has moved since the last mousemove
                dy = my - startY;

            let isInsideRect = false;

            // move each rect that isDragging 
            // by the distance the mouse has moved
            // since the last mousemove
            for (let r of settings.RectArray) {
                if (settings.DragOk && r.IsDragging) {
                    r.X += dx;
                    r.Y += dy;
                    if (r.Type === "prc") isChange = true;
                }
                // Changing cursor if mouse is inside a rectangle.
                if (r.IsShow && !isInsideRect && mx > r.X && mx < r.X + r.Width && my > r.Y && my < r.Y + r.Height) {
                    this.style.cursor = "grab";
                    isInsideRect = true;
                }
            }

            if (!isInsideRect) this.style.cursor = "default";

            // redraw the scene with the new rect positions
            draw();

            // reset the starting mouse position for the next mousemove
            startX = mx;
            startY = my;
            //}
        }

        function myDown(e) {
            // tell the browser we're handling this mouse event
            e.preventDefault();
            e.stopPropagation();

            let anySelect = false;

            // Clearing selected employee
            selectedEmp = null;

            // get the current mouse position
            const bc = this.getBoundingClientRect(),
                mx = parseInt(e.clientX - bc.left),
                my = parseInt(e.clientY - bc.top);

            // test each rect to see if mouse is inside
            settings.DragOk = false;
            for (let r of settings.RectArray) {
                if (mx > r.X && mx < r.X + r.Width && my > r.Y && my < r.Y + r.Height) {
                    anySelect = true;

                    if (e.ctrlKey) {
                        // If pressing ctrl and there is no red rectangle, fill red.
                        if (!isRedRect) {
                            r.BorderColor = "red";
                            isRedRect = true;
                        } else {
                            // If pressing ctrl and there is a red rectangle, fill blue.
                            r.BorderColor = "#00008b";
                        }
                    } else {
                        // If not pressing ctrl, fill red.
                        r.BorderColor = "red";
                        isRedRect = true;
                    }

                    // if yes, set that RectArray IsDragging=true
                    settings.DragOk = true;

                    r.IsDragging = true;

                    // Selected rectangle is an employee and allow displaying                  
                    if (r.Type === "emp" && r.IsShow) {
                        selectedEmp = r;
                    }

                    if (r.Data.Emp && r.Type === "prc") {
                        // Draw image by: ctx.drawImage(imgUser, x, y + 78, 40, 40); ( image width, height = 40 ) so
                        // Inside user image
                        if (mx > r.X && mx < r.X + 40 && my > r.Y + 78 && my < r.Y + 78 + 40) {
                            // If user just want to move employee out of process
                            r.IsDragging = false;

                            unAssignEmp(r.Data.Emp, mx - 50, my - 60); // 50 is width/2, 60 is height/2

                            // Removing employee to process
                            //console.log("Unassigned employee");
                            isChange = true;

                            r.Data.Emp = null;

                            //// redrawing to show the employee
                            //draw();
                        }
                    }
                } else {
                    // If not pressing ctrl, resetting other rectangles to default color border.
                    if (!e.ctrlKey) {
                        r.BorderColor = r.TextColor;
                    }
                }
            }

            // Resetting border color if no selected rectangle.
            if (!anySelect) {
                settings.RectArray.map(x => x.BorderColor = x.TextColor);
                isRedRect = false;
            }

            // redrawing to show the employee and border color
            draw();

            // save the current mouse position
            startX = mx;
            startY = my;
        }

        // Un-assigning an employee to process
        function unAssignEmp(emp, x, y) {
            const existedEmp = settings.RectArray.find(x => x.Type === "emp" && x.Data.EmployeeCode === emp.EmployeeCode);

            if (existedEmp) {
                // Updating employee
                existedEmp.X = x;
                existedEmp.Y = y;
                existedEmp.IsShow = true;
            } else {
                // Creating an employee then inserting to the array for drawing
                const newEmp = new RectObj("emp", x, y, 100, 120, "#5588ee", "#FAF7F8", "#5588ee", false, true, emp);
                //console.log(newEmp);

                settings.RectArray.push(newEmp);
            }
        }

        function myUp(e) {
            // tell the browser we're handling this mouse event
            e.preventDefault();
            e.stopPropagation();

            // clear all the dragging flags
            settings.DragOk = false;

            let isAssignEmp = false;

            for (let r of settings.RectArray) {
                r.IsDragging = false;

                // Detecting intersection
                // 1. If there is an selected employee
                // 2. Dragging employee to process
                // 3. Still not assign within this action
                // 4. Which employees were not assigned and hidden
                // 5. Process is not assigned by an employee
                if (selectedEmp && r.Type === "prc" && isAssignEmp === false && r.IsShow && (r.Data.Emp === null || r.Data.Emp === undefined)) {
                    const r1 = {
                        left: selectedEmp.X,
                        top: selectedEmp.Y,
                        right: selectedEmp.X + selectedEmp.Width,
                        bottom: selectedEmp.Y + selectedEmp.Height
                    },
                        r2 = {
                            left: r.X,
                            top: r.Y,
                            right: r.X + r.Width,
                            bottom: r.Y + r.Height
                        },
                        rs = intersectRect(r1, r2);

                    if (rs === true) {
                        isAssignEmp = true;
                        isChange = true;

                        //console.log(`Assigned to the process: ${r.Data.OpNum}`);

                        // Not displaying (not drawing) the employee anymore
                        selectedEmp.IsShow = false;

                        // Assigning employee to process
                        r.Data.Emp = selectedEmp.Data;

                        // Re-drawing rectangles
                        draw();
                    }
                }
            }
        }

        //function myWheel(e) {
        //    if (event.ctrlKey) {
        //        // tell the browser we're handling this mouse event
        //        e.preventDefault();
        //        e.stopPropagation();

        //        if (e.deltaY < 0) {
        //            console.log("Mouse up!");

        //            //this.width = this.width * 0.8;
        //            //this.height = this.height * 0.8;
        //            //console.log(this.width);
        //            //console.log(this.height);

        //            //settings.CanvasWidth = this.width;
        //            //settings.CanvasHeight = this.height;

        //            settings.ScaleX = 1.2;
        //            settings.ScaleY = 1.2;
        //            //console.log(settings.ScaleX);
        //            //console.log(settings.ScaleY);

        //            //draw();
        //            //ctx.translate(5, 5);
        //        }
        //        else if (e.deltaY > 0) {
        //            //this.width = this.width / 1.5;
        //            //this.height = this.height / 1.5;
        //            console.log("Mouse down!");

        //            //this.width = this.width * 1.2;
        //            //this.height = this.height * 1.2;
        //            //console.log(this.width);
        //            //console.log(this.height);

        //            //settings.CanvasWidth = this.width;
        //            //settings.CanvasHeight = this.height;

        //            settings.ScaleX = 0.8;
        //            settings.ScaleY = 0.8;

        //            //ctx.translate(-5, -5);

        //            //ctx.scale(settings.ScaleX, settings.ScaleY);

        //            //console.log(settings.ScaleX);
        //            //console.log(settings.ScaleY);

        //            //// redrawing one zoom
        //            //draw();
        //        }

        //        //ctx.scale(settings.ScaleX, settings.ScaleY);
        //        //console.log(new Date());
        //        //for (let r of settings.RectArray) {
        //            //r.X *= settings.ScaleX;
        //            //r.Y *= settings.ScaleY;

        //            //r.X += r.X * settings.ScaleX;
        //            //r.Y += r.Y * settings.ScaleX;

        //            //r.Width *= settings.ScaleX;
        //            //r.Height *= settings.ScaleY;
        //            //r.Width += 5;
        //            //r.Height += 5;
        //        //}
        //        //console.log(new Date());

        //        // redrawing once zoom
        //        draw();
        //    } else {
        //        console.log("The CTRL key was NOT pressed!");
        //    }
        //}

        //function myResize() {
        //    console.log("myResize");
        //}

        function onZoomIn() {
            //settings.ScaleX = 1.2;
            //settings.ScaleY = 1.2;

            ctx.scale(settings.ScaleX, settings.ScaleY);
            draw();

            //for (let r of settings.RectArray) {
            //    r.X *= 1.2;
            //    r.Y *= 1.2;
            //    r.Width *= 1.2;
            //    r.Height *= 1.2;
            //}
        }

        function bindData(data) {
            if (data) {
                settings.RectArray = data;
                draw();
            }
        }

        function reloadEmp(data) {
            // Removing all of employee
            const prcArr = settings.RectArray.filter(x => x.Type === "prc");

            // Inserting new employee data
            settings.RectArray = prcArr.concat(data);
        }

        function clickConDiv() {
            ///<summary>
            /// Clicking event of div that contain canvas
            /// Disable/enable saving button
            ///</summary >

            document.getElementById(btnOpEmpSaveChanges).disabled = !isChange;
        }
    };
}(jQuery));
