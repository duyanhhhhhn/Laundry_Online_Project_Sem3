function CalendarPicker(element, options) {
    this.date = new Date();
    this._formatDateToInit(this.date);
    this.day = this.date.getDay()
    this.month = this.date.getMonth();
    this.year = this.date.getFullYear();
    this.today = this.date;
    this.value = this.date;
    this.min = options.min;
    this.max = options.max;
    this._formatDateToInit(this.min);
    this._formatDateToInit(this.max);
    this.userElement = document.querySelector(element);
    this._setDateText();
    this.calendarWrapper = document.createElement('div');
    this.calendarElement = document.createElement('div')
    this.calendarHeader = document.createElement('header');
    this.calendarHeaderTitle = document.createElement('div');
    this.navigationWrapper = document.createElement('div')
    this.previousMonthArrow = document.createElement('button');
    this.nextMonthArrow = document.createElement('button');
    this.calendarGridDays = document.createElement('section')
    this.calendarGrid = document.createElement('section');
    this.calendarDayElementType = 'time';
    this.listOfAllDaysAsText = [
        'M',
        'T',
        'W',
        'T',
        'F',
        'S',
        'S'
    ];
    this.listOfAllMonthsAsText = [
        'Tháng 1',
        'Tháng 2',
        'Tháng 3',
        'Tháng 4',
        'Tháng 5',
        'Tháng 6',
        'Tháng 7',
        'Tháng 8',
        'Tháng 9',
        'Tháng 10',
        'Tháng 11',
        'Tháng 12'
    ];
    this.calendarWrapper.id = 'calendar-wrapper';
    this.calendarElement.id = 'calendar';
    this.calendarGridDays.id = 'calendar-days';
    this.calendarGrid.id = 'calendar-grid';
    this.navigationWrapper.id = 'navigation-wrapper';
    this.previousMonthArrow.id = 'previous-month';
    this.nextMonthArrow.id = 'next-month';
    this._insertHeaderIntoCalendarWrapper();
	this._insertNavigationButtons();
    this._insertCalendarGridDaysHeader();
    this._insertDaysIntoGrid();
    this._insertCalendarIntoWrapper();
    this.userElement.appendChild(this.calendarWrapper);
}
CalendarPicker.prototype._getDaysInMonth = function (month, year) {
    if ((!month && month !== 0) || (!year && year !== 0)) return;
    var date = new Date(year, month, 1);
    var days = [];
    while (date.getMonth() === month) {
        days.push(new Date(date));
        date.setDate(date.getDate() + 1);
    }
    return days;
}
CalendarPicker.prototype._formatDateToInit = function (date) {
    if (!date) return;
    date.setHours(0, 0, 0);
}
CalendarPicker.prototype._setDateText = function () {
    var dateData = this.date.toString().split(' ');
    this.dayAsText = dateData[0];
    this.monthAsText = dateData[1];
    this.dateAsText = dateData[2];
    this.yearAsText = dateData[3];
}
CalendarPicker.prototype._insertCalendarIntoWrapper = function () {
    this.calendarWrapper.appendChild(this.calendarElement);
    var handleSelectedElement = (event) => {
        if (event.target.nodeName.toLowerCase() === this.calendarDayElementType && !event.target.classList.contains('disabled')) {
            Array.from(document.querySelectorAll('.selected')).forEach(element => element.classList.remove('selected'));
            event.target.classList.add('selected');
            this.value = event.target.value;
            this.onValueChange(this.callback);
        }
    }
    this.calendarGrid.addEventListener('click', handleSelectedElement, false);
    this.calendarGrid.addEventListener('keydown', (keyEvent) => {
        if (keyEvent.key !== 'Enter') return;
        handleSelectedElement(keyEvent);
    }, false);
}
CalendarPicker.prototype._insertHeaderIntoCalendarWrapper = function () {
    this.calendarHeaderTitle.textContent = `${this.listOfAllMonthsAsText[this.month]} - ${this.year}`;
    this.calendarHeader.appendChild(this.calendarHeaderTitle);
    this.calendarWrapper.appendChild(this.calendarHeader);
}
CalendarPicker.prototype._insertCalendarGridDaysHeader = function () {
    this.listOfAllDaysAsText.forEach(day => {
        var dayElement = document.createElement('span');
        dayElement.textContent = day;
        this.calendarGridDays.appendChild(dayElement);
    })
    this.calendarElement.appendChild(this.calendarGridDays);
}
CalendarPicker.prototype._insertNavigationButtons = function () {
    var arrowSvg = '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:svgjs="http://svgjs.com/svgjs" version="1.1" x="0" y="0" viewBox="0 0 443.52 443.52" style="enable-background:new 0 0 512 512" xml:space="preserve" class=""><path d="M143.492,221.863L336.226,29.129c6.663-6.664,6.663-17.468,0-24.132c-6.665-6.662-17.468-6.662-24.132,0l-204.8,204.8    c-6.662,6.664-6.662,17.468,0,24.132l204.8,204.8c6.78,6.548,17.584,6.36,24.132-0.42c6.387-6.614,6.387-17.099,0-23.712    L143.492,221.863z" fill="#ffffff" data-original="#000000" style=""/></svg>';
    this.previousMonthArrow.innerHTML = arrowSvg;
    this.nextMonthArrow.innerHTML = arrowSvg;
    this.previousMonthArrow.setAttribute('aria-label', 'Go to previous month');
    this.nextMonthArrow.setAttribute('aria-label', 'Go to next month');
    this.navigationWrapper.appendChild(this.previousMonthArrow);
    this.navigationWrapper.appendChild(this.nextMonthArrow);
    var that = this;
    this.navigationWrapper.addEventListener('click', function (clickEvent) {
        if (clickEvent.target.closest(`#${that.previousMonthArrow.id}`)) {
            if (that.month === 0) {
                that.month = 11;
                that.year -= 1;
            } else {
                that.month -= 1;
            }
            that._updateCalendar();
        }
        if (clickEvent.target.closest(`#${that.nextMonthArrow.id}`)) {
            if (that.month === 11) {
                that.month = 0;
                that.year += 1;
            } else {
                that.month += 1;
            }
            that._updateCalendar();
        }
    }, false)

    that.calendarElement.appendChild(that.navigationWrapper);
}
CalendarPicker.prototype._insertDaysIntoGrid = function () {
    this.calendarGrid.innerHTML = '';
    var arrayOfDays = this._getDaysInMonth(this.month, this.year);
    var firstDayOfMonth = arrayOfDays[0].getDay();
    firstDayOfMonth = firstDayOfMonth === 0 ? 7 : firstDayOfMonth;
    if (1 < firstDayOfMonth) {
        arrayOfDays = Array(firstDayOfMonth - 1).fill(false, 0).concat(arrayOfDays);
    }
    arrayOfDays.forEach(date => {
        var dateElement = document.createElement(date ? this.calendarDayElementType : 'span');
        var Date = date.toString().split(' ')[2];
        var dateIsTheCurrentValue = this.value.toString() === date.toString();
        if (dateIsTheCurrentValue) this.activeDateElement = dateElement;
        var dateIsBetweenAllowedRange = (this.min || this.max) && (date.toString() !== this.today.toString() && (date < this.min || date > this.max))
        if (dateIsBetweenAllowedRange) {
            dateElement.classList.add('disabled');
        } else {
            dateElement.tabIndex = 0;
            dateElement.value = date;
        }
        dateElement.textContent = date ? `${Date}` : '';
        this.calendarGrid.appendChild(dateElement);
    })
    this.calendarElement.appendChild(this.calendarGrid);
    this.activeDateElement.classList.add('selected');
}
CalendarPicker.prototype._updateCalendar = function () {
    this.date = new Date(this.year, this.month);
    this._setDateText();
    this.day = this.date.getDay();
    this.month = this.date.getMonth();
    this.year = this.date.getFullYear();
    var that = this;
    window.requestAnimationFrame(function () {
        that.calendarHeaderTitle.textContent = `${that.listOfAllMonthsAsText[that.month]} - ${that.year}`;
        that._insertDaysIntoGrid();
    })
}
CalendarPicker.prototype.onValueChange = function (callback) {
    if (this.callback) return this.callback(this.value);
    this.callback = callback;
}