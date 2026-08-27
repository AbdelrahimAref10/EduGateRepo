import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { manageUsersGuard } from './core/auth/permission.guard';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ParentDashboardComponent } from './features/dashboards/parent-dashboard/parent-dashboard';
import { ParentAttendanceComponent } from './features/parent/parent-attendance';
import { ParentChildrenComponent } from './features/parent/parent-children';
import { ParentExamsComponent } from './features/parent/parent-exams';
import { ParentPaymentsComponent } from './features/parent/parent-payments';
import { StudentDashboardComponent } from './features/dashboards/student-dashboard/student-dashboard';
import { SuperAdminDashboardComponent } from './features/dashboards/super-admin-dashboard/super-admin-dashboard';
import { TeacherDashboardComponent } from './features/dashboards/teacher-dashboard/teacher-dashboard';
import { ShellLayoutComponent } from './features/layouts/shell-layout/shell-layout';
import { EditProfileComponent } from './features/profile/edit-profile/edit-profile';
import { StudentClassroomComponent } from './features/student/classroom/student-classroom';
import { StudentExamsComponent } from './features/student/classroom/student-exams';
import { StudentLessonDetailComponent } from './features/student/lessons/student-lesson-detail';
import { StudentLessonsComponent } from './features/student/lessons/student-lessons';
import { StudentMyLessonsComponent } from './features/student/lessons/student-my-lessons';
import { AdminEducationComponent } from './features/super-admin/education/admin-education';
import { AdminCountriesComponent } from './features/super-admin/countries/admin-countries';

import { AdminBillingComponent } from './features/super-admin/billing/admin-billing';
import { AdminLessonsComponent } from './features/super-admin/lessons/admin-lessons';
import { AdminClassroomComponent } from './features/super-admin/groups/admin-classroom';
import { AdminGroupsComponent } from './features/super-admin/groups/admin-groups';
import { AdminUsersComponent } from './features/super-admin/users/admin-users';

import { LandingComponent } from './features/landing/landing';
import { PublicLayoutComponent } from './features/marketplace/public-layout';
import { MarketplaceDiscoverComponent } from './features/marketplace/marketplace-discover';
import { StudentDiscoverComponent } from './features/student/discover/student-discover';
import { StudentTeacherComponent } from './features/student/discover/student-teacher';
import { PublicTeacherComponent } from './features/marketplace/public-teacher';
import { PublicLessonRedirectComponent } from './features/marketplace/public-lesson-redirect';
import { TeacherBookingsComponent } from './features/teacher/bookings/teacher-bookings';
import { TeacherReviewsComponent } from './features/teacher/reviews/teacher-reviews';
import { TeacherClassroomComponent } from './features/teacher/classroom/teacher-classroom';
import { TeacherGroupManageComponent } from './features/teacher/lessons/teacher-group-manage';
import { TeacherLessonManageComponent } from './features/teacher/lessons/teacher-lesson-manage';
import { TeacherLessonsComponent } from './features/teacher/lessons/teacher-lessons';
import { TeacherPaymentsComponent } from './features/teacher/payments/teacher-payments';
import { TeacherStudentGroupComponent } from './features/teacher/students/teacher-student-group';
import { TeacherStudentLessonsComponent } from './features/teacher/students/teacher-student-lessons';
import { TeacherStudentsComponent } from './features/teacher/students/teacher-students';

const routes: Routes = [
  { path: '', pathMatch: 'full', component: LandingComponent },
  {
    path: 'login',
    canActivate: [guestGuard],
    component: LoginComponent,
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    component: RegisterComponent,
  },
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: 'discover', component: MarketplaceDiscoverComponent },
      { path: 't/:teacherId', component: PublicTeacherComponent },
      { path: 'l/:lessonId', component: PublicLessonRedirectComponent },
    ],
  },
  {
    path: 'super-admin',
    canActivate: [authGuard, roleGuard(['SuperAdmin'])],
    component: ShellLayoutComponent,
    data: {
      roleKey: 'auth.roleAdmin',
      accent: 'admin',
      homeLink: '/super-admin',
    },
    children: [
      { path: '', component: SuperAdminDashboardComponent },
      { path: 'users', canActivate: [manageUsersGuard], component: AdminUsersComponent },
      { path: 'lessons', component: AdminLessonsComponent },
      { path: 'groups', component: AdminGroupsComponent },
      { path: 'classroom/:sessionId', component: AdminClassroomComponent },
      { path: 'billing', component: AdminBillingComponent },
      { path: 'education', component: AdminEducationComponent },
      { path: 'countries', component: AdminCountriesComponent },
      { path: 'profile', component: EditProfileComponent },
    ],
  },
  {
    path: 'teacher',
    canActivate: [authGuard, roleGuard(['Teacher'])],
    component: ShellLayoutComponent,
    data: {
      roleKey: 'auth.roleTeacher',
      accent: 'teacher',
      homeLink: '/teacher',
    },
    children: [
      { path: '', component: TeacherDashboardComponent },
      { path: 'students', component: TeacherStudentsComponent },
      { path: 'students/:studentId', component: TeacherStudentLessonsComponent },
      { path: 'students/:studentId/lessons/:lessonId', component: TeacherStudentGroupComponent },
      { path: 'lessons', component: TeacherLessonsComponent },
      { path: 'lessons/:lessonId', component: TeacherLessonManageComponent },
      { path: 'lessons/:lessonId/groups/:groupId', component: TeacherGroupManageComponent },
      { path: 'classroom/:sessionId', component: TeacherClassroomComponent },
      { path: 'payments', component: TeacherPaymentsComponent },
      { path: 'bookings', component: TeacherBookingsComponent },
      { path: 'reviews', component: TeacherReviewsComponent },
      { path: 'profile', component: EditProfileComponent },
    ],
  },
  {
    path: 'student',
    canActivate: [authGuard, roleGuard(['Student'])],
    component: ShellLayoutComponent,
    data: {
      roleKey: 'auth.roleStudent',
      accent: 'student',
      homeLink: '/student',
    },
    children: [
      { path: '', component: StudentDashboardComponent },
      { path: 'lessons', component: StudentMyLessonsComponent },
      { path: 'lessons/:lessonId', component: StudentLessonDetailComponent },
      { path: 'book', component: StudentLessonsComponent },
      { path: 'exams', component: StudentExamsComponent },
      { path: 'discover', component: StudentDiscoverComponent },
      { path: 'discover/:teacherId', component: StudentTeacherComponent },
      { path: 'classroom/:sessionId', component: StudentClassroomComponent },
      { path: 'profile', component: EditProfileComponent },
    ],
  },
  {
    path: 'parent',
    canActivate: [authGuard, roleGuard(['Parent'])],
    component: ShellLayoutComponent,
    data: {
      roleKey: 'auth.roleParent',
      accent: 'parent',
      homeLink: '/parent',
    },
    children: [
      { path: '', component: ParentDashboardComponent },
      { path: 'children', component: ParentChildrenComponent },
      { path: 'exams', component: ParentExamsComponent },
      { path: 'attendance', component: ParentAttendanceComponent },
      { path: 'payments', component: ParentPaymentsComponent },
      { path: 'profile', component: EditProfileComponent },
    ],
  },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
