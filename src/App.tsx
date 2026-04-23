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

    const fullName = prompt("Введите Фамилия Имя Отчество:", "Иванов Иван Иванович");
    if (!fullName) return;
    const parts = fullName.trim().split(/\s+/);
    if (parts.length < 3) {
      setStatusMessage("Нужно ввести Фамилия Имя Отчество.");
      return;
    }
    const [lastName, firstName, ...middle] = parts;
    const classTitle = prompt("Введите класс (например, 8А):", classes[0]?.title ?? "8А");
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

    const fullName = prompt(
      "Введите Фамилия Имя Отчество через пробел:",
      `${selected.lastName} ${selected.firstName} ${selected.middleName}`,
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
    const title = prompt("Введите название класса (например, 8А):", "8А");
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
    const selected = classes[classesIndex];
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
    const selected = classes[classesIndex];
    if (!selected) return;
    const title = prompt("Введите новое название класса (например, 8А):", selected.title);
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
    const name = prompt("Введите название предмета:", "Математика");
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
    const selected = subjects[subjectsIndex];
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
    const selected = subjects[subjectsIndex];
    if (!selected) return;
    const name = prompt("Введите новое название предмета:", selected.name);
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
    const fullName = prompt("Введите Фамилия Имя Отчество учителя:", "Иванова Мария Петровна");
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
    const selected = teachers[teachersIndex];
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
    const selected = teachers[teachersIndex];
    if (!selected) return;
    const fullName = prompt("Введите Фамилия Имя Отчество:", selected.fullName);
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
    const classTitle = prompt("Класс (например, 8А):", classes[0]?.title ?? "8А");
    if (!classTitle) return;
    const classId = classes.find((c) => c.title.toUpperCase() === classTitle.trim().toUpperCase())?.id;
    if (!classId) {
      setStatusMessage("Класс не найден.");
      return;
    }
    const day = prompt("День недели (Понедельник..Пятница):", "Понедельник");
    if (!day) return;
    const lessonNumberRaw = prompt("Номер урока (1-10):", "1");
    if (!lessonNumberRaw) return;
    const lessonNumber = Number(lessonNumberRaw);
    if (!Number.isInteger(lessonNumber) || lessonNumber < 1 || lessonNumber > 10) {
      setStatusMessage("Номер урока должен быть целым числом от 1 до 10.");
      return;
    }
    const subjectName = prompt("Предмет:", subjects[0]?.name ?? "Математика");
    if (!subjectName) return;
    const subjectId = subjects.find((s) => s.name.toLowerCase() === subjectName.trim().toLowerCase())?.id;
    if (!subjectId) {
      setStatusMessage("Предмет не найден.");
      return;
    }
    const teacherFullName = prompt("ФИО учителя:", teachers[0]?.fullName ?? "");
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
    const selected = schedule[scheduleIndex];
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
    const selected = schedule[scheduleIndex];
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
    const studentFullName = prompt("ФИО ученика:", studentNameById(students[0]?.id ?? 0));
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
    const subjectName = prompt("Предмет:", subjects[0]?.name ?? "Математика");
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
    const selected = grades[gradesIndex];
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
    const selected = grades[gradesIndex];
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
        <p className="subtitle">
          Многооконный интерфейс для проверки требований: CRUD, поиск, сортировка, навигация, lookup-поля.
        </p>
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
            rows={classes.map((c) => [c.title])}
            index={classesIndex}
            setIndex={setClassesIndex}
            onAdd={createClass}
            onEdit={editSelectedClass}
            onDelete={deleteSelectedClass}
          />
        )}

        {activePage === "directories" && openWindows.includes("subjects") && (
          <SimpleWindow
            title="Форма: Предметы"
            headers={["Название"]}
            rows={subjects.map((s) => [s.name])}
            index={subjectsIndex}
            setIndex={setSubjectsIndex}
            onAdd={createSubject}
            onEdit={editSelectedSubject}
            onDelete={deleteSelectedSubject}
          />
        )}

        {activePage === "directories" && openWindows.includes("teachers") && (
          <SimpleWindow
            title="Форма: Учителя"
            headers={["ФИО"]}
            rows={teachers.map((t) => [t.fullName])}
            index={teachersIndex}
            setIndex={setTeachersIndex}
            onAdd={createTeacher}
            onEdit={editSelectedTeacher}
            onDelete={deleteSelectedTeacher}
          />
        )}

        {activePage === "journal" && openWindows.includes("schedule") && (
          <SimpleWindow
            title="Форма: Расписание"
            headers={["Класс", "День", "Урок", "Предмет", "Учитель"]}
            rows={schedule.map((s) => [
              classNameById(s.classId),
              dayToUi[s.day] ?? s.day,
              String(s.lessonNumber),
              subjectNameById(s.subjectId),
              teacherNameById(s.teacherId),
            ])}
            index={scheduleIndex}
            setIndex={setScheduleIndex}
            onAdd={createScheduleItem}
            onEdit={editSelectedScheduleItem}
            onDelete={deleteSelectedScheduleItem}
          />
        )}

        {activePage === "journal" && openWindows.includes("grades") && (
          <SimpleWindow
            title="Форма: Журнал оценок"
            headers={["Ученик", "Предмет", "Дата", "Оценка"]}
            rows={grades.map((g) => [studentNameById(g.studentId), subjectNameById(g.subjectId), g.date, String(g.value)])}
            index={gradesIndex}
            setIndex={setGradesIndex}
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
  onAdd: () => void;
  onDelete: () => void;
  onEdit?: () => void;
};

function SimpleWindow({ title, headers, rows, index, setIndex, onAdd, onDelete, onEdit }: SimpleWindowProps) {
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
