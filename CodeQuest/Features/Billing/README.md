# Billing

`Plans.aspx` is the learner-facing subscription page. It follows the prototype's
two plans:

- Basic: RM0 forever and beginner-course access.
- Premium: RM29/month and access to beginner, intermediate and advanced courses.

The Premium button is a simulated checkout for this academic prototype. It does
not collect card details. `BillingRepository.ActivatePremium` writes the
`Subscription` and `Payment` records in one SQL transaction and updates
`dbo.User.[plan]` to `Premium`.

The ERD also shows a possible one-time course payment. That is separate from
this subscription flow and should only be added after `Payment` gains a
`CourseID` or `EnrollmentID` relationship.
