import { useEffect, useMemo, useState } from "react";
import "./App.css";

type Student = {
  id: number;
  lastName: string;
  firstName: string;
  middleName: string;
  classId: number;
};

type SchoolClass = { id: number; title: string };
type Subject = { id: number; name: string };
type Teacher = { id: number; fullName: string };
type Schedule = {
  id: number;
  classId: number;
  day: string;
  lessonNumber: number;
  subjectId: number;
  teacherId: number;
};
type Grade = {
  id: number;
  studentId: number;
  subjectId: number;
  date: string;
  value: number;
};

type WindowKey = "students" | "classes" | "subjects" | "teachers" | "schedule" | "grades";
type PageKey = "journal" | "directories";

const WINDOW_TITLES: Record<WindowKey, string> = {
  students: "Ученики",
  classes: "Классы",
  subjects: "Предметы",
  teachers: "Учителя",
  schedule: "Расписание",
  grades: "Оценки",
};

function inputWithSuggestions(
  title: string,
  initialValue = "",
  suggestions: string[] = [],
): Promise<string | null> {
  return new Promise((resolve) => {
    const backdrop = document.createElement("div");
    backdrop.className = "suggest-backdrop";

    const dialog = document.createElement("div");
    dialog.className = "suggest-dialog";

    const heading = document.createElement("h3");
    heading.textContent = title;

    const input = document.createElement("input");
    input.type = "text";
    input.value = initialValue;
    input.className = "suggest-input";
    const listId = `suggest-list-${Math.random().toString(36).slice(2)}`;
    input.setAttribute("list", listId);

    const datalist = document.createElement("datalist");
    datalist.id = listId;
    [...new Set(suggestions)].forEach((s) => {
      const option = document.createElement("option");
      option.value = s;
      datalist.appendChild(option);
    });

    const actions = document.createElement("div");
    actions.className = "suggest-actions";

    const okBtn = document.createElement("button");
    okBtn.textContent = "OK";
    const cancelBtn = document.createElement("button");
    cancelBtn.textContent = "Отмена";

    const cleanup = () => {
      document.removeEventListener("keydown", onKeyDown);
      backdrop.remove();
    };

    const onCancel = () => {
      cleanup();
      resolve(null);
    };

    const onOk = () => {
      const value = input.value.trim();
      cleanup();
      resolve(value.length ? value : null);
    };

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onCancel();
      if (e.key === "Enter") onOk();
    };

    okBtn.onclick = onOk;
    cancelBtn.onclick = onCancel;
    backdrop.onclick = (e) => {
      if (e.target === backdrop) onCancel();
    };

    actions.append(okBtn, cancelBtn);
    dialog.append(heading, input, datalist, actions);
    backdrop.appendChild(dialog);
    document.body.appendChild(backdrop);
    input.focus();
    input.select();
    document.addEventListener("keydown", onKeyDown);
  });
}

function App() {
  const [students, setStudents] = useState<Student[]>([]);
  const [classes, setClasses] = useState<SchoolClass[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [teachers, setTeachers] = useState<Teacher[]>([]);
  const [schedule, setSchedule] = useState<Schedule[]>([]);
  const [grades, setGrades] = useState<Grade[]>([]);
  const [statusMessage, setStatusMessage] = useState("");
  const [activePage, setActivePage] = useState<PageKey>("journal");

  const [openWindows, setOpenWindows] = useState<WindowKey[]>(["students", "schedule"]);
  const [studentSearch, setStudentSearch] = useState("");
  const [classSearch, setClassSearch] = useState("");
  const [subjectSearch, setSubjectSearch] = useState("");
  const [teacherSearch, setTeacherSearch] = useState("");
  const [scheduleSearch, setScheduleSearch] = useState("");
  const [gradesSearch, setGradesSearch] = useState("");
  const [studentSort, setStudentSort] = useState<"Фамилия" | "Класс">("Фамилия");

  const [studentsIndex, setStudentsIndex] = useState(0);
  const [classesIndex, setClassesIndex] = useState(0);
  const [subjectsIndex, setSubjectsIndex] = useState(0);
  const [teachersIndex, setTeachersIndex] = useState(0);
  const [scheduleIndex, setScheduleIndex] = useState(0);
  const [gradesIndex, setGradesIndex] = useState(0);

  const classNameById = (id: number) => classes.find((x) => x.id === id)?.title ?? "—";
  const subjectNameById = (id: number) => subjects.find((x) => x.id === id)?.name ?? "—";
  const studentNameById = (id: number) => {
    const s = students.find((x) => x.id === id);
    return s ? `${s.lastName} ${s.firstName} ${s.middleName}` : "—";
  };
  const teacherNameById = (id: number) => teachers.find((x) => x.id === id)?.fullName ?? "—";

  const sortedAndFilteredStudents = useMemo(() => {
    const search = studentSearch.trim().toLowerCase();
    let result = [...students];
    if (search) {
      result = result.filter((s) => `${s.lastName} ${s.firstName} ${s.middleName}`.toLowerCase().includes(search));
    }
    result.sort((a, b) =>
      studentSort === "Фамилия"
        ? a.lastName.localeCompare(b.lastName, "ru")
        : classNameById(a.classId).localeCompare(classNameById(b.classId), "ru"),
    );
    return result;
  }, [students, studentSearch, studentSort, classes]);

  const locatorStudentId = useMemo(() => {
    const val = studentSearch.trim().toLowerCase();
    if (!val) return null;
    return sortedAndFilteredStudents.find((s) => s.lastName.toLowerCase().startsWith(val))?.id ?? null;
  }, [studentSearch, sortedAndFilteredStudents]);

  const averageGradeBySubject = useMemo(() => {
    const acc = new Map<number, { sum: number; count: number }>();
    for (const g of grades) {
      const cur = acc.get(g.subjectId) ?? { sum: 0, count: 0 };
      cur.sum += g.value;
      cur.count += 1;
      acc.set(g.subjectId, cur);
    }

    return [...acc.entries()]
      .map(([subjectId, { sum, count }]) => ({
        subjectId,
        subjectName: subjectNameById(subjectId),
        avg: sum / count,
      }))
      .sort((a, b) => a.subjectName.localeCompare(b.subjectName, "ru"));
  }, [grades, subjects]);

  const filteredClasses = useMemo(() => {
    const q = classSearch.trim().toLowerCase();
    if (!q) return classes;
    return classes.filter((x) => x.title.toLowerCase().includes(q));
  }, [classes, classSearch]);

  const filteredSubjects = useMemo(() => {
    const q = subjectSearch.trim().toLowerCase();
    if (!q) return subjects;
    return subjects.filter((x) => x.name.toLowerCase().includes(q));
  }, [subjects, subjectSearch]);

  const filteredTeachers = useMemo(() => {
    const q = teacherSearch.trim().toLowerCase();
    if (!q) return teachers;
    return teachers.filter((x) => x.fullName.toLowerCase().includes(q));
  }, [teachers, teacherSearch]);

  const filteredSchedule = useMemo(() => {
    const q = scheduleSearch.trim().toLowerCase();
    if (!q) return schedule;
    return schedule.filter((s) =>
      [
        classNameById(s.classId),
        dayToUi[s.day] ?? s.day,
        String(s.lessonNumber),
        subjectNameById(s.subjectId),
        teacherNameById(s.teacherId),
      ]
        .join(" ")
        .toLowerCase()
        .includes(q),
    );
  }, [schedule, scheduleSearch, classes, subjects, teachers]);

  const filteredGrades = useMemo(() => {
    const q = gradesSearch.trim().toLowerCase();
    if (!q) return grades;
    return grades.filter((g) =>
      [studentNameById(g.studentId), subjectNameById(g.subjectId), g.date, String(g.value)]
        .join(" ")
        .toLowerCase()
        .includes(q),
    );
  }, [grades, gradesSearch, students, subjects]);

  const toggleWindow = (key: WindowKey) => {
    setOpenWindows((prev) =>
      prev.includes(key) ? prev.filter((x) => x !== key) : [...prev, key],
    );
  };

  const moveIndex = (current: number, size: number, mode: "first" | "prev" | "next" | "last") => {
    if (size === 0) return 0;
    if (mode === "first") return 0;
    if (mode === "last") return size - 1;
    if (mode === "prev") return Math.max(current - 1, 0);
    return Math.min(current + 1, size - 1);
  };

  async function exportAllTablesToExcel() {
    const safe = (v: string | number) =>
      String(v)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    const makeTable = (title: string, headers: string[], rows: (string | number)[][]) => `
      <h2>${safe(title)}</h2>
      <table border="1" cellspacing="0" cellpadding="4">
        <thead><tr>${headers.map((h) => `<th>${safe(h)}</th>`).join("")}</tr></thead>
        <tbody>${rows.map((r) => `<tr>${r.map((c) => `<td>${safe(c)}</td>`).join("")}</tr>`).join("")}</tbody>
      </table>
      <br/>
    `;

    const html = `
      <html xmlns:o="urn:schemas-microsoft-com:office:office"
            xmlns:x="urn:schemas-microsoft-com:office:excel"
            xmlns="http://www.w3.org/TR/REC-html40">
      <head><meta charset="utf-8" /></head>
      <body>
        ${makeTable("Ученики", ["Фамилия", "Имя", "Отчество", "Класс"], students.map((s) => [s.lastName, s.firstName, s.middleName, classNameById(s.classId)]))}
        ${makeTable("Классы", ["Название"], classes.map((x) => [x.title]))}
        ${makeTable("Предметы", ["Название"], subjects.map((x) => [x.name]))}
        ${makeTable("Учителя", ["ФИО"], teachers.map((x) => [x.fullName]))}
        ${makeTable("Расписание", ["Класс", "День", "Урок", "Предмет", "Учитель"], schedule.map((s) => [classNameById(s.classId), dayToUi[s.day] ?? s.day, s.lessonNumber, subjectNameById(s.subjectId), teacherNameById(s.teacherId)]))}
        ${makeTable("Журнал оценок", ["Ученик", "Предмет", "Дата", "Оценка"], grades.map((g) => [studentNameById(g.studentId), subjectNameById(g.subjectId), g.date, g.value]))}
      </body>
      </html>`;

    const filename = `school_tables_${new Date().toISOString().slice(0, 10)}.xls`;
    const blob = new Blob([`\uFEFF${html}`], { type: "application/vnd.ms-excel;charset=utf-8;" });

    try {
      if ("showSaveFilePicker" in window) {
        const handle = await (window as any).showSaveFilePicker({
          suggestedName: filename,
          types: [
            {
              description: "Excel file",
              accept: { "application/vnd.ms-excel": [".xls"] },
            },
          ],
        });
        const writable = await handle.createWritable();
        await writable.write(blob);
        await writable.close();
        setStatusMessage("Файл Excel сохранён.");
        return;
      }
    } catch {
      // Пользователь закрыл диалог или API недоступно — используем обычную загрузку ниже.
    }

    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    a.click();
    URL.revokeObjectURL(a.href);
    setStatusMessage("Экспорт в Excel выполнен.");
  }

  const apiBase = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? "http://localhost:5050";
  const pageWindows: Record<PageKey, WindowKey[]> = {
    journal: ["students", "schedule", "grades"],
    directories: ["classes", "subjects", "teachers"],
  };
  const dayToUi: Record<string, string> = {
    Monday: "Понедельник",
    Tuesday: "Вторник",
    Wednesday: "Среда",
    Thursday: "Четверг",
    Friday: "Пятница",
    Понедельник: "Понедельник",
    Вторник: "Вторник",
    Среда: "Среда",
    Четверг: "Четверг",
    Пятница: "Пятница",
  };
  const dayToApi: Record<string, string> = {
    Понедельник: "Monday",
    Вторник: "Tuesday",
    Среда: "Wednesday",
    Четверг: "Thursday",
    Пятница: "Friday",
    Monday: "Monday",
    Tuesday: "Tuesday",
    Wednesday: "Wednesday",
    Thursday: "Thursday",
    Friday: "Friday",
  };

  async function loadData() {
    try {
      const response = await fetch(`${apiBase}/api/data`);
      if (!response.ok) {
        throw new Error("Сервер вернул ошибку.");
      }
      const data = await response.json();
      setClasses(data.classes ?? []);
      setSubjects(data.subjects ?? []);
      setTeachers(data.teachers ?? []);
      setStudents(data.students ?? []);
      setSchedule((data.schedule ?? []).map((s: any) => ({ ...s, day: dayToUi[String(s.day)] ?? String(s.day) })));
      setGrades((data.grades ?? []).map((g: any) => ({ ...g, date: String(g.date).slice(0, 10) })));
      setStatusMessage("Данные загружены из backend API.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? `Ошибка подключения к backend API: ${error.message}` : "Ошибка подключения к backend API.");
    }
  }

  useEffect(() => {
    loadData();
  }, []);

  async function reseedDefaultData() {
    try {
      const response = await fetch(`${apiBase}/api/admin/reseed-default`, { method: "POST" });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось восстановить базовые данные.");
      }
      await loadData();
      setStatusMessage("Базовые данные восстановлены.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка восстановления базовых данных.");
    }
  }

  async function createStudent() {
    if (!classes.length) {
      setStatusMessage("Сначала должны быть созданы классы.");
      return;
    }

    const fullName = await inputWithSuggestions(
      "Введите Фамилия Имя Отчество:",
      "Иванов Иван Иванович",
      students.map((s) => `${s.lastName} ${s.firstName} ${s.middleName}`),
    );
    if (!fullName) return;
    const parts = fullName.trim().split(/\s+/);
    if (parts.length < 3) {
      setStatusMessage("Нужно ввести Фамилия Имя Отчество.");
      return;
    }
    const [lastName, firstName, ...middle] = parts;
    const classTitle = await inputWithSuggestions(
      "Введите класс (например, 8А):",
      classes[0]?.title ?? "8А",
      classes.map((c) => c.title),
    );
    if (!classTitle) return;
    const classId = classes.find((c) => c.title.toUpperCase() === classTitle.trim().toUpperCase())?.id;
    if (!classId) {
      setStatusMessage("Класс не найден.");
      return;
    }

    try {
      const response = await fetch(`${apiBase}/api/students`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          lastName,
          firstName,
          middleName: middle.join(" "),
          birthDate: "2011-09-01",
          studentStatusId: 1,
          classId,
        }),
      });

      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Ошибка добавления ученика.");
      }

      await loadData();
      setStatusMessage("Ученик добавлен через use-case backend.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка добавления ученика.");
    }
  }

  async function deleteSelectedStudent() {
    const selected = sortedAndFilteredStudents[studentsIndex];
    if (!selected) {
      return;
    }
    try {
      const response = await fetch(`${apiBase}/api/students/${selected.id}`, { method: "DELETE" });
      if (!response.ok && response.status !== 204) {
        throw new Error("Не удалось удалить ученика.");
      }
      await loadData();
      setStudentsIndex(0);
      setStatusMessage("Ученик удален.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Не удалось удалить ученика на сервере.");
    }
  }

  async function editSelectedStudent() {
    const selected = sortedAndFilteredStudents[studentsIndex];
    if (!selected) return;

    const fullName = await inputWithSuggestions(
      "Введите Фамилия Имя Отчество через пробел:",
      `${selected.lastName} ${selected.firstName} ${selected.middleName}`,
      students.map((s) => `${s.lastName} ${s.firstName} ${s.middleName}`),
    );
    if (!fullName) return;
    const parts = fullName.trim().split(/\s+/);
    if (parts.length < 3) {
      setStatusMessage("Нужно ввести Фамилия Имя Отчество.");
      return;
    }
    const [lastName, firstName, ...middleRest] = parts;
    const middleName = middleRest.join(" ");

    try {
      const response = await fetch(`${apiBase}/api/students/${selected.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          lastName,
          firstName,
          middleName,
          birthDate: "2011-09-01",
        }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось изменить ученика.");
      }
      await loadData();
      setStatusMessage("Данные ученика обновлены.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка обновления ученика.");
    }
  }

  async function createClass() {
    const title = await inputWithSuggestions(
      "Введите название класса (например, 8А):",
      "8А",
      classes.map((c) => c.title),
    );
    if (!title) return;
    try {
      const response = await fetch(`${apiBase}/api/classes`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title: title.trim().toUpperCase() }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось добавить класс.");
      }
      await loadData();
      setStatusMessage("Класс добавлен.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка добавления класса.");
    }
  }

  async function deleteSelectedClass() {
    const selected = filteredClasses[classesIndex];
    if (!selected) return;
    try {
      const response = await fetch(`${apiBase}/api/classes/${selected.id}`, { method: "DELETE" });
      if (!response.ok && response.status !== 204) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось удалить класс.");
      }
      await loadData();
      setClassesIndex(0);
      setStatusMessage("Класс удален.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка удаления класса.");
    }
  }

  async function editSelectedClass() {
    const selected = filteredClasses[classesIndex];
    if (!selected) return;
    const title = await inputWithSuggestions(
      "Введите новое название класса (например, 8А):",
      selected.title,
      classes.map((c) => c.title),
    );
    if (!title) return;
    const nextTitle = title.trim();
    if (!nextTitle) return;
    try {
      const response = await fetch(`${apiBase}/api/classes/${selected.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title: nextTitle }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось изменить класс.");
      }
      await loadData();
      setStatusMessage("Класс обновлен.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка обновления класса.");
    }
  }

  async function createSubject() {
    const name = await inputWithSuggestions(
      "Введите название предмета:",
      "Математика",
      subjects.map((s) => s.name),
    );
    if (!name) return;
    try {
      const response = await fetch(`${apiBase}/api/subjects`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: name.trim() }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось добавить предмет.");
      }
      await loadData();
      setStatusMessage("Предмет добавлен.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка добавления предмета.");
    }
  }

  async function deleteSelectedSubject() {
    const selected = filteredSubjects[subjectsIndex];
    if (!selected) return;
    try {
      const response = await fetch(`${apiBase}/api/subjects/${selected.id}`, { method: "DELETE" });
      if (!response.ok && response.status !== 204) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось удалить предмет.");
      }
      await loadData();
      setSubjectsIndex(0);
      setStatusMessage("Предмет удален.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка удаления предмета.");
    }
  }

  async function editSelectedSubject() {
    const selected = filteredSubjects[subjectsIndex];
    if (!selected) return;
    const name = await inputWithSuggestions(
      "Введите новое название предмета:",
      selected.name,
      subjects.map((s) => s.name),
    );
    if (!name) return;
    const nextName = name.trim();
    if (!nextName) return;
    try {
      const response = await fetch(`${apiBase}/api/subjects/${selected.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: nextName }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось изменить предмет.");
      }
      await loadData();
      setStatusMessage("Предмет обновлен.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка обновления предмета.");
    }
  }

  async function createTeacher() {
    const fullName = await inputWithSuggestions(
      "Введите Фамилия Имя Отчество учителя:",
      "Иванова Мария Петровна",
      teachers.map((t) => t.fullName),
    );
    if (!fullName) return;
    const parts = fullName.trim().split(/\s+/);
    if (parts.length < 3) {
      setStatusMessage("Нужно ввести Фамилия Имя Отчество.");
      return;
    }
    const [lastName, firstName, ...middle] = parts;
    try {
      const response = await fetch(`${apiBase}/api/teachers`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          lastName,
          firstName,
          middleName: middle.join(" "),
        }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось добавить учителя.");
      }
      await loadData();
      setStatusMessage("Учитель добавлен.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка добавления учителя.");
    }
  }

  async function deleteSelectedTeacher() {
    const selected = filteredTeachers[teachersIndex];
    if (!selected) return;
    try {
      const response = await fetch(`${apiBase}/api/teachers/${selected.id}`, { method: "DELETE" });
      if (!response.ok && response.status !== 204) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось удалить учителя.");
      }
      await loadData();
      setTeachersIndex(0);
      setStatusMessage("Учитель удален.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка удаления учителя.");
    }
  }

  async function editSelectedTeacher() {
    const selected = filteredTeachers[teachersIndex];
    if (!selected) return;
    const fullName = await inputWithSuggestions(
      "Введите Фамилия Имя Отчество:",
      selected.fullName,
      teachers.map((t) => t.fullName),
    );
    if (!fullName) return;
    const parts = fullName.trim().split(/\s+/);
    if (parts.length < 3) {
      setStatusMessage("Нужно ввести Фамилия Имя Отчество.");
      return;
    }
    const [lastName, firstName, ...middle] = parts;
    try {
      const response = await fetch(`${apiBase}/api/teachers/${selected.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ lastName, firstName, middleName: middle.join(" ") }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось изменить учителя.");
      }
      await loadData();
      setStatusMessage("Учитель обновлен.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка обновления учителя.");
    }
  }

  async function createScheduleItem() {
    if (!classes.length || !subjects.length || !teachers.length) {
      setStatusMessage("Для добавления расписания нужны классы, предметы и учителя.");
      return;
    }
    const classTitle = await inputWithSuggestions(
      "Класс (например, 8А):",
      classes[0]?.title ?? "8А",
      classes.map((c) => c.title),
    );
    if (!classTitle) return;
    const classId = classes.find((c) => c.title.toUpperCase() === classTitle.trim().toUpperCase())?.id;
    if (!classId) {
      setStatusMessage("Класс не найден.");
      return;
    }
    const day = await inputWithSuggestions(
      "День недели (Понедельник..Пятница):",
      "Понедельник",
      ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница"],
    );
    if (!day) return;
    const lessonNumberRaw = prompt("Номер урока (1-10):", "1");
    if (!lessonNumberRaw) return;
    const lessonNumber = Number(lessonNumberRaw);
    if (!Number.isInteger(lessonNumber) || lessonNumber < 1 || lessonNumber > 10) {
      setStatusMessage("Номер урока должен быть целым числом от 1 до 10.");
      return;
    }
    const subjectName = await inputWithSuggestions(
      "Предмет:",
      subjects[0]?.name ?? "Математика",
      subjects.map((s) => s.name),
    );
    if (!subjectName) return;
    const subjectId = subjects.find((s) => s.name.toLowerCase() === subjectName.trim().toLowerCase())?.id;
    if (!subjectId) {
      setStatusMessage("Предмет не найден.");
      return;
    }
    const teacherFullName = await inputWithSuggestions(
      "ФИО учителя:",
      teachers[0]?.fullName ?? "",
      teachers.map((t) => t.fullName),
    );
    if (!teacherFullName) return;
    const teacherId = teachers.find((t) => t.fullName.toLowerCase() === teacherFullName.trim().toLowerCase())?.id;
    if (!teacherId) {
      setStatusMessage("Учитель не найден.");
      return;
    }
    try {
      const response = await fetch(`${apiBase}/api/schedule`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          classId,
          day: dayToApi[day] ?? day,
          lessonNumber,
          subjectId,
          teacherId,
        }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось добавить запись расписания.");
      }
      await loadData();
      setStatusMessage("Запись расписания добавлена.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка добавления расписания.");
    }
  }

  async function deleteSelectedScheduleItem() {
    const selected = filteredSchedule[scheduleIndex];
    if (!selected) return;
    try {
      const response = await fetch(`${apiBase}/api/schedule/${selected.id}`, { method: "DELETE" });
      if (!response.ok && response.status !== 204) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось удалить запись расписания.");
      }
      await loadData();
      setScheduleIndex(0);
      setStatusMessage("Запись расписания удалена.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка удаления расписания.");
    }
  }

  async function editSelectedScheduleItem() {
    const selected = filteredSchedule[scheduleIndex];
    if (!selected) return;
    const lessonNumber = prompt("Введите номер урока (1-10):", String(selected.lessonNumber));
    if (!lessonNumber) return;
    const num = Number(lessonNumber);
    if (!Number.isInteger(num) || num < 1 || num > 10) {
      setStatusMessage("Номер урока должен быть целым числом от 1 до 10.");
      return;
    }
    try {
      const response = await fetch(`${apiBase}/api/schedule/${selected.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          classId: selected.classId,
          day: dayToApi[selected.day] ?? "Thursday",
          lessonNumber: num,
          subjectId: selected.subjectId,
          teacherId: selected.teacherId,
        }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось изменить расписание.");
      }
      await loadData();
      setStatusMessage("Запись расписания обновлена.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка обновления расписания.");
    }
  }

  async function createGradeItem() {
    if (!students.length || !subjects.length) {
      setStatusMessage("Для добавления оценки нужны ученики и предметы.");
      return;
    }
    const studentFullName = await inputWithSuggestions(
      "ФИО ученика:",
      studentNameById(students[0]?.id ?? 0),
      students.map((s) => `${s.lastName} ${s.firstName} ${s.middleName}`),
    );
    if (!studentFullName) return;
    const student = students.find(
      (s) =>
        `${s.lastName} ${s.firstName} ${s.middleName}`.toLowerCase() ===
        studentFullName.trim().toLowerCase(),
    );
    if (!student) {
      setStatusMessage("Ученик не найден.");
      return;
    }
    const subjectName = await inputWithSuggestions(
      "Предмет:",
      subjects[0]?.name ?? "Математика",
      subjects.map((s) => s.name),
    );
    if (!subjectName) return;
    const subjectId = subjects.find((s) => s.name.toLowerCase() === subjectName.trim().toLowerCase())?.id;
    if (!subjectId) {
      setStatusMessage("Предмет не найден.");
      return;
    }
    const date = prompt("Дата урока (YYYY-MM-DD):", "2026-02-10");
    if (!date) return;
    const valueRaw = prompt("Оценка (1-5):", "5");
    if (!valueRaw) return;
    const value = Number(valueRaw);
    if (!Number.isInteger(value) || value < 1 || value > 5) {
      setStatusMessage("Оценка должна быть целым числом от 1 до 5.");
      return;
    }
    try {
      const response = await fetch(`${apiBase}/api/grades`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          studentId: student.id,
          subjectId,
          date,
          value,
        }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось добавить оценку.");
      }
      await loadData();
      setStatusMessage("Оценка добавлена.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка добавления оценки.");
    }
  }

  async function deleteSelectedGradeItem() {
    const selected = filteredGrades[gradesIndex];
    if (!selected) return;
    try {
      const response = await fetch(`${apiBase}/api/grades/${selected.id}`, { method: "DELETE" });
      if (!response.ok && response.status !== 204) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось удалить оценку.");
      }
      await loadData();
      setGradesIndex(0);
      setStatusMessage("Оценка удалена.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка удаления оценки.");
    }
  }

  async function editSelectedGradeItem() {
    const selected = filteredGrades[gradesIndex];
    if (!selected) return;
    const value = prompt("Введите новую оценку (1-5):", String(selected.value));
    if (!value) return;
    const numeric = Number(value);
    if (!Number.isInteger(numeric) || numeric < 1 || numeric > 5) {
      setStatusMessage("Оценка должна быть целым числом от 1 до 5.");
      return;
    }
    try {
      const response = await fetch(`${apiBase}/api/grades/${selected.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ value: numeric }),
      });
      if (!response.ok) {
        const err = await response.json().catch(() => null);
        throw new Error(err?.message ?? "Не удалось изменить оценку.");
      }
      await loadData();
      setStatusMessage("Оценка обновлена.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Ошибка обновления оценки.");
    }
  }

  return (
    <main className="app">
      <header className="header">
        <h1>Школа (Успеваемость)</h1>
        <p className="status">{statusMessage}</p>
        <button onClick={reseedDefaultData}>Восстановить базовый сидинг</button>
      </header>

      <section className="page-switcher">
        <button
          className={activePage === "journal" ? "active" : ""}
          onClick={() => setActivePage("journal")}
        >
          Форма 1: Учебный процесс
        </button>
        <button
          className={activePage === "directories" ? "active" : ""}
          onClick={() => setActivePage("directories")}
        >
          Форма 2: Справочники
        </button>
      </section>

      <section className="window-switcher">
        {pageWindows[activePage].map((key) => (
          <label key={key} className="window-toggle">
            <input type="checkbox" checked={openWindows.includes(key)} onChange={() => toggleWindow(key)} />
            {WINDOW_TITLES[key]}
          </label>
        ))}
      </section>

      <section className="top-actions">
        <button onClick={exportAllTablesToExcel}>Печать в Excel: все таблицы</button>
      </section>

      <section className="chart-panel">
        <h2>График: средний балл по предметам</h2>
        {averageGradeBySubject.length === 0 ? (
          <p className="chart-empty">Нет данных для построения графика.</p>
        ) : (
          <svg className="line-chart" viewBox="0 0 860 220" role="img" aria-label="График средних оценок по предметам">
            <line x1="30" y1="190" x2="840" y2="190" className="line-chart-axis" />
            <line x1="30" y1="20" x2="30" y2="190" className="line-chart-axis" />
            <polyline
              className="line-chart-polyline"
              points={averageGradeBySubject
                .map((item, i) => {
                  const x = 30 + (i * (810 / Math.max(1, averageGradeBySubject.length - 1)));
                  const y = 190 - (item.avg / 5) * 170;
                  return `${x},${y}`;
                })
                .join(" ")}
            />
            {averageGradeBySubject.map((item, i) => {
              const x = 30 + (i * (810 / Math.max(1, averageGradeBySubject.length - 1)));
              const y = 190 - (item.avg / 5) * 170;
              return (
                <g key={item.subjectId}>
                  <circle cx={x} cy={y} r="4" className="line-chart-point" />
                  <text x={x} y={208} className="line-chart-label" textAnchor="middle">
                    {item.subjectName.length > 12 ? `${item.subjectName.slice(0, 12)}…` : item.subjectName}
                  </text>
                  <text x={x} y={y - 8} className="line-chart-value" textAnchor="middle">
                    {item.avg.toFixed(2)}
                  </text>
                </g>
              );
            })}
          </svg>
        )}
      </section>

      <section className="windows-grid">
        {activePage === "journal" && openWindows.includes("students") && (
          <article className="window">
            <h2>Форма: Ученики</h2>
            <div className="tools">
              <input
                value={studentSearch}
                onChange={(e) => setStudentSearch(e.currentTarget.value)}
                placeholder="Инкрементальный поиск (например: Март)"
              />
              <select value={studentSort} onChange={(e) => setStudentSort(e.currentTarget.value as "Фамилия" | "Класс")}>
                <option>Фамилия</option>
                <option>Класс</option>
              </select>
              <button onClick={createStudent}>
                Добавить
              </button>
              <button onClick={editSelectedStudent}>Изменить</button>
              <button onClick={deleteSelectedStudent}>
                Удалить
              </button>
            </div>
            <div className="nav">
              <button onClick={() => setStudentsIndex((i) => moveIndex(i, sortedAndFilteredStudents.length, "first"))}>В начало</button>
              <button onClick={() => setStudentsIndex((i) => moveIndex(i, sortedAndFilteredStudents.length, "prev"))}>Назад</button>
              <button onClick={() => setStudentsIndex((i) => moveIndex(i, sortedAndFilteredStudents.length, "next"))}>Вперед</button>
              <button onClick={() => setStudentsIndex((i) => moveIndex(i, sortedAndFilteredStudents.length, "last"))}>В конец</button>
            </div>
            <table>
              <thead>
                <tr>
                  <th>Фамилия</th>
                  <th>Имя</th>
                  <th>Отчество</th>
                  <th>Класс</th>
                </tr>
              </thead>
              <tbody>
                {sortedAndFilteredStudents.map((s, idx) => (
                  <tr
                    key={s.id}
                    className={[
                      idx === studentsIndex ? "selected" : "",
                      locatorStudentId === s.id ? "locator" : "",
                    ].join(" ")}
                    onClick={() => setStudentsIndex(idx)}
                  >
                    <td>{s.lastName}</td>
                    <td>{s.firstName}</td>
                    <td>{s.middleName}</td>
                    <td>{classNameById(s.classId)}</td>
                  </tr>
                ))}
                {Array.from({ length: Math.max(0, 10 - sortedAndFilteredStudents.length) }).map((_, idx) => (
                  <tr key={`students-empty-${idx}`}>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </article>
        )}

        {activePage === "directories" && openWindows.includes("classes") && (
          <SimpleWindow
            title="Форма: Классы"
            headers={["Название"]}
            rows={filteredClasses.map((c) => [c.title])}
            index={classesIndex}
            setIndex={setClassesIndex}
            searchTerm={classSearch}
            setSearchTerm={setClassSearch}
            searchPlaceholder="Поиск по классам..."
            onAdd={createClass}
            onEdit={editSelectedClass}
            onDelete={deleteSelectedClass}
          />
        )}

        {activePage === "directories" && openWindows.includes("subjects") && (
          <SimpleWindow
            title="Форма: Предметы"
            headers={["Название"]}
            rows={filteredSubjects.map((s) => [s.name])}
            index={subjectsIndex}
            setIndex={setSubjectsIndex}
            searchTerm={subjectSearch}
            setSearchTerm={setSubjectSearch}
            searchPlaceholder="Поиск по предметам..."
            onAdd={createSubject}
            onEdit={editSelectedSubject}
            onDelete={deleteSelectedSubject}
          />
        )}

        {activePage === "directories" && openWindows.includes("teachers") && (
          <SimpleWindow
            title="Форма: Учителя"
            headers={["ФИО"]}
            rows={filteredTeachers.map((t) => [t.fullName])}
            index={teachersIndex}
            setIndex={setTeachersIndex}
            searchTerm={teacherSearch}
            setSearchTerm={setTeacherSearch}
            searchPlaceholder="Поиск по учителям..."
            onAdd={createTeacher}
            onEdit={editSelectedTeacher}
            onDelete={deleteSelectedTeacher}
          />
        )}

        {activePage === "journal" && openWindows.includes("schedule") && (
          <SimpleWindow
            title="Форма: Расписание"
            headers={["Класс", "День", "Урок", "Предмет", "Учитель"]}
            rows={filteredSchedule.map((s) => [
              classNameById(s.classId),
              dayToUi[s.day] ?? s.day,
              String(s.lessonNumber),
              subjectNameById(s.subjectId),
              teacherNameById(s.teacherId),
            ])}
            index={scheduleIndex}
            setIndex={setScheduleIndex}
            searchTerm={scheduleSearch}
            setSearchTerm={setScheduleSearch}
            searchPlaceholder="Поиск по расписанию..."
            onAdd={createScheduleItem}
            onEdit={editSelectedScheduleItem}
            onDelete={deleteSelectedScheduleItem}
          />
        )}

        {activePage === "journal" && openWindows.includes("grades") && (
          <SimpleWindow
            title="Форма: Журнал оценок"
            headers={["Ученик", "Предмет", "Дата", "Оценка"]}
            rows={filteredGrades.map((g) => [studentNameById(g.studentId), subjectNameById(g.subjectId), g.date, String(g.value)])}
            index={gradesIndex}
            setIndex={setGradesIndex}
            searchTerm={gradesSearch}
            setSearchTerm={setGradesSearch}
            searchPlaceholder="Поиск по оценкам..."
            onAdd={createGradeItem}
            onDelete={deleteSelectedGradeItem}
            onEdit={editSelectedGradeItem}
          />
        )}
      </section>
    </main>
  );
}

type SimpleWindowProps = {
  title: string;
  headers: string[];
  rows: string[][];
  index: number;
  setIndex: React.Dispatch<React.SetStateAction<number>>;
  searchTerm: string;
  setSearchTerm: React.Dispatch<React.SetStateAction<string>>;
  searchPlaceholder: string;
  onAdd: () => void;
  onDelete: () => void;
  onEdit?: () => void;
};

function SimpleWindow({
  title,
  headers,
  rows,
  index,
  setIndex,
  searchTerm,
  setSearchTerm,
  searchPlaceholder,
  onAdd,
  onDelete,
  onEdit,
}: SimpleWindowProps) {
  const moveIndex = (current: number, size: number, mode: "first" | "prev" | "next" | "last") => {
    if (size === 0) return 0;
    if (mode === "first") return 0;
    if (mode === "last") return size - 1;
    if (mode === "prev") return Math.max(current - 1, 0);
    return Math.min(current + 1, size - 1);
  };

  return (
    <article className="window">
      <h2>{title}</h2>
      <div className="tools">
        <input
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.currentTarget.value)}
          placeholder={searchPlaceholder}
        />
        <button onClick={onAdd}>Добавить</button>
        {onEdit ? <button onClick={onEdit}>Изменить</button> : null}
        <button onClick={onDelete}>Удалить</button>
      </div>
      <div className="nav">
        <button onClick={() => setIndex((i) => moveIndex(i, rows.length, "first"))}>В начало</button>
        <button onClick={() => setIndex((i) => moveIndex(i, rows.length, "prev"))}>Назад</button>
        <button onClick={() => setIndex((i) => moveIndex(i, rows.length, "next"))}>Вперед</button>
        <button onClick={() => setIndex((i) => moveIndex(i, rows.length, "last"))}>В конец</button>
      </div>
      <table>
        <thead>
          <tr>{headers.map((h) => <th key={h}>{h}</th>)}</tr>
        </thead>
        <tbody>
          {rows.map((row, rowIdx) => (
            <tr key={`${title}-${rowIdx}`} className={rowIdx === index ? "selected" : ""} onClick={() => setIndex(rowIdx)}>
              {row.map((cell, cellIdx) => (
                <td key={`${rowIdx}-${cellIdx}`}>{cell}</td>
              ))}
            </tr>
          ))}
          {Array.from({ length: Math.max(0, 10 - rows.length) }).map((_, rowIdx) => (
            <tr key={`${title}-empty-${rowIdx}`}>
              {headers.map((header) => (
                <td key={`${title}-empty-${rowIdx}-${header}`}>&nbsp;</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </article>
  );
}

export default App;
