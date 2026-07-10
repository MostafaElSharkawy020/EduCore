# EduCore — User Manual

This manual explains how to use EduCore as a **Student** or a **Teacher**.

- **Live app:** http://educoreasp.runasp.net/
- **Demo teacher:** email `teacher@educore.local`, password `Teacher@123`

---

## 1. Getting Started

1. Open the app URL. You'll land on the **Login** page.
2. If you already have an account, log in. Otherwise click **Sign up**.

### Creating an account (Sign up)
1. Click **Sign up** on the login page.
2. Fill in first name, last name, email, phone, and a password (min 6 characters).
3. Choose **I am a: Student** or **Teacher**.
4. Click **Create Account** — you're signed in and taken to your home page.

> Emails must be unique. Use the 👁️ button to reveal your password while typing.

### Logging in
1. On the **Login** page, choose the **Student** or **Teacher** tab.
2. Enter your email and password → **Sign In**.
3. Teachers land on the **Dashboard**; students land on **My Courses**.

### Logging out
Click **Logout** (top-right of the page).

### Your profile
Click your **avatar** (top-right) or **My Profile** in the sidebar:
- **Personal Info** — view your details
- **Edit Profile** — change name, email, phone (teachers also have a Biography)
- **Password** — change your password (enter current + new)

---

## 2. Student Guide

### Browse courses
- **Browse Courses** (sidebar) shows all courses open for enrollment.
- Use the search box to filter by course or teacher name.
- Click a course to open its **details** page (curriculum, instructor, price).

### Enroll in a course
On a course's details page:
- **Free course** → **Enroll Now (Free)**.
- **Paid course** → **Enroll — EGP X** → the **Checkout** page.
- **Already enrolled** → **Go to Course**.

### Enroll in a single class (à la carte)
Some courses let you buy **individual classes** instead of the whole course.
On the course details page, each class row shows:
- **Open** — you already have access
- **Enroll — EGP X** or **Enroll (Free)** — buy just that class
- **Enroll in the course** — this class isn't sold separately

> Some courses are **class-only** (the whole-course button is hidden) — you buy classes individually.

### Paying (Checkout)
1. Paid enrollment opens **Checkout**.
2. Select a saved payment card, then click **Pay EGP X**.
3. If you have no card, click **Add a card** first (see below). Payment is **simulated** — no real charge.
4. After paying you're enrolled and taken into the course.

### Payment cards
**Payment Cards** (sidebar):
- **Add Card** — cardholder name, card number, expiry (MM/YY), CVV.
- View saved cards (shown masked, e.g. `•••• 4242`) and remove them.
- See your **Purchase History** (item, amount, card, date).

### My Courses & learning
- **My Courses** lists everything you're enrolled in (full courses and individually-owned classes).
- Open a course → see its **classes** and **exams**.
- Open a class → **watch videos**, **download PDFs** (lecture notes / homework), and see its **quizzes**.

### Taking a quiz or exam
1. From a class, click **Take Quiz** (or from a course, **Take Exam**).
2. If there's a **time limit**, a countdown appears and the quiz **auto-submits** when it hits 0:00.
3. Select one answer per question → **Submit**.
4. You'll see your **score** and **per-question feedback** (correct answer in green, your wrong pick flagged).
5. You can **Retake** — the class/course page shows your **last score**.

---

## 3. Teacher Guide

### Dashboard
After login, the **Dashboard** shows:
- Totals: courses, students, classes, quizzes & exams
- Your course cards
- **Recent Student Activity** — latest quiz/exam attempts by your students

### Create & manage courses
- **New Course** (sidebar) → name, price, and toggles:
  - **Open for Enrollment** — list it in the catalog
  - **Allow buying the whole course** — turn **off** to make it **class-only**
- **My Courses** → **Edit**, **+ Class**, **Exams**, or **Delete** per course.

### Classes
- From a course, **+ Class** (or **New Class**) → name, price, **Open for individual enrollment** toggle, and PDF links (lecture notes / homework).
- Each class can hold **videos** and **quizzes**.

### Videos
- On a class, **Videos** → **+ Add Video** → title + URL.
- Paste a **YouTube**, **Vimeo**, or direct **.mp4** link — it embeds automatically.

### Quizzes (per class)
1. On a class, **Quizzes** → **+ New Quiz** → title + **time limit (minutes)** (0 = no limit).
2. **Manage Questions** → **+ Add Question**:
   - Question text
   - Answer choices — **one per line**
   - **Correct choice number** (e.g. `2` = the second line)
3. **View Results** to see who took it, their scores, and the average.

### Exams (per course)
Same as quizzes, but attached to a **course** and reached via a course's **Exams** button.

### Results
On any quiz/exam, **View Results** (or the **Results** link) shows:
- Sales/attempt stats: attempts, distinct students, average %
- A table of each student's score, %, and date

### Revenue
**Revenue** (sidebar) shows your earnings from sales:
- Gross revenue, platform fee, and **your earnings** (you keep **80%**, platform takes **20%**)
- A sales history with your share per sale

---

## 4. Tips & Notes
- **Currency** is Egyptian Pound (EGP) throughout.
- **Payments are simulated** for this project — never enter a real card number.
- If a page says "access denied," you're signed in with the wrong role for that area.
- Forgot to enroll? Content pages are locked until you enroll in the course or that class.
