/* jspanel.contextmenu.js (c) Stefan Sträßer(Flyer53) <info@jspanel.de> license: MIT */

/* global jsPanel */
'use strict'; //import {jsPanel} from '../../jspanel.js';

if (!jsPanel.contextmenu) {
  jsPanel.contextmenu = {
    version: '1.1.1',
    date: '2019-05-18 10:15',
    defaults: {
      //position: is set in jsPanel.contextmenu.create()
      //container: is set in jsPanel.contextmenu.create()
      dragit: false,
      resizeit: false,
      header: false,
      headerControls: 'none'
    },
    cmOverflow: function cmOverflow(elmt) {
      var cltX = elmt.cmEvent.clientX,
          cltY = elmt.cmEvent.clientY,
          panelW = elmt.offsetWidth,
          panelH = elmt.offsetHeight,
          corrLeft = window.innerWidth - (cltX + panelW),
          corrTop = window.innerHeight - (cltY + panelH);

      if (corrLeft < 0) {
        elmt.style.left = cltX + (window.scrollX || window.pageXOffset) - panelW + 'px';
      }

      if (corrTop < 0) {
        elmt.style.top = cltY + (window.scrollY || window.pageYOffset) - panelH + 'px';
      }
    },
    create: function create() {
      var _this = this;

      var options = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : {};
      var evt = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : 'contextmenu';
      options.paneltype = 'contextmenu';
      var target = options.target;

      if (!target) {
        return false;
      }

      if (typeof target === 'string') {
        target = document.querySelector(target);
      }

      target.addEventListener(evt, function (e) {
        e.preventDefault(); // close all contextmenus first

        document.querySelectorAll('.jsPanel-contextmenu').forEach(function (item) {
          item.close();
        });
        var l = (e.pageX || e.touches[0].pageX) + 'px',
            t = (e.pageY || e.touches[0].pageY) + 'px',
            opts = options;

        if (options.config) {
          opts = Object.assign({}, options.config, options);
          delete opts.config;
        }

        opts = Object.assign({}, _this.defaults, opts, {
          position: false,
          container: 'body'
        });
        jsPanel.create(opts, function (cm) {
          jsPanel.setStyle(cm, {
            position: 'absolute',
            left: l,
            top: t
          }); // check whether contextmenu is triggered from within a modal panel or panel and if so update z-index

          var closestModal = target.closest('.jsPanel-modal');

          if (closestModal) {
            cm.style.zIndex = closestModal.style.zIndex;
          } else {
            var closestPanel = target.closest('.jsPanel');

            if (closestPanel) {
              closestPanel.front();
            }

            cm.style.zIndex = jsPanel.zi.next();
          } // save event object as property of cm outer div (needed in checkContextmenuOverflow())


          cm.cmEvent = e; // update left/top values if menu overflows browser viewport

          jsPanel.contextmenu.cmOverflow(cm);
          cm.addEventListener('mouseleave', function () {
            cm.close();
          }, false); // don't close contextmenu on mousedown in target

          cm.addEventListener(jsPanel.evtStart, function (e) {
            e.stopPropagation();
          }, false);
        });
      }, false);
    }
  }; // add overflow check to jsPanel.contentAjax always callback

  jsPanel.ajaxAlwaysCallbacks.push(function (obj) {
    if (obj.classList.contains('jsPanel-contextmenu')) {
      jsPanel.contextmenu.cmOverflow(obj);
    }
  }); // close tooltips on pointerdown in document

  jsPanel.pointerdown.forEach(function (evt) {
    document.addEventListener(evt, function (e) {
      document.querySelectorAll('.jsPanel-contextmenu').forEach(function (item) {
        if (!e.target.closest('.jsPanel-contextmenu')) {
          item.close();
        }
      });
    }, false);
  });
} // Add CommonJS module exports, so it can be imported using require() in Node.js
// https://nodejs.org/docs/latest/api/modules.html


if (typeof module !== 'undefined') {
  module.exports = jsPanel;
}